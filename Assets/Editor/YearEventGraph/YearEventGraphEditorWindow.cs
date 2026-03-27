using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class YearEventGraphEditorWindow : EditorWindow
{
    private YearEventGraphView graphView;
    private List<YearEventData> eventDataList = new List<YearEventData>();
    
    private const string LAYOUT_FILE_PATH = "Assets/Editor/YearEventGraph/YearEventGraphLayout.json";

    [MenuItem("Tools/Year Event Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<YearEventGraphEditorWindow>();
        window.titleContent = new GUIContent("Year Event Graph");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        CreateGraphView();
        CreateToolbar();
        LoadAllYearEvents();
        
        EditorApplication.wantsToQuit += OnEditorQuit;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        if (graphView != null && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SaveNodePositionsToFile();
            rootVisualElement.Remove(graphView);
        }
        
        EditorApplication.wantsToQuit -= OnEditorQuit;
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private bool OnEditorQuit()
    {
        SaveNodePositionsToFile();
        Debug.Log("[YearEventGraph] Layout saved before editor quit");
        return true;
    }

    private void OnBeforeAssemblyReload()
    {
        if (graphView != null)
        {
            SaveNodePositionsToFile();
            Debug.Log("[YearEventGraph] Layout auto-saved before script recompile");
        }
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && graphView != null)
        {
            SaveNodePositionsToFile();
            Debug.Log("[YearEventGraph] Layout saved before entering Play Mode");
        }
        
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            Debug.Log("[YearEventGraph] Returned to Edit Mode, reloading graph...");
            EditorApplication.delayCall += () =>
            {
                if (this != null && graphView != null)
                {
                    RefreshGraph();
                }
            };
        }
    }

    private void CreateGraphView()
    {
        graphView = new YearEventGraphView(this)
        {
            name = "Year Event Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void CreateToolbar()
    {
        var toolbar = new YearEventToolbar();

        var refreshButton = new Button(() => RefreshGraph()) { text = "Refresh" };
        refreshButton.tooltip = "Reload all events and restore saved layout";
        toolbar.Add(refreshButton);

        var saveButton = new Button(() => SaveConnections()) { text = "Save All" };
        saveButton.tooltip = "Save connections and node positions to file";
        toolbar.Add(saveButton);

        var resetLayoutButton = new Button(() => ResetLayout()) { text = "Reset Layout" };
        resetLayoutButton.tooltip = "Reset all positions to grid layout sorted by name (can be reverted via Git)";
        toolbar.Add(resetLayoutButton);

        rootVisualElement.Add(toolbar);
    }

    private void ResetLayout()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Layout",
            "모든 노드의 위치가 초기화되고 이름 순으로 재배치됩니다.\n\n" +
            "이 작업은 Git으로 되돌릴 수 있습니다.\n\n" +
            "정말로 진행하시겠습니까?",
            "확인",
            "취소"
        );

        if (!confirmed)
        {
            Debug.Log("[YearEventGraph] Layout reset cancelled");
            return;
        }

        Dictionary<string, Rect> emptyPositions = new Dictionary<string, Rect>();
        graphView.RefreshGraphWithPositions(eventDataList, emptyPositions);

        SaveNodePositionsToFile();

        Debug.Log($"[YearEventGraph] Layout reset: {eventDataList.Count} nodes repositioned in grid");
    }

    private void LoadAllYearEvents()
    {
        eventDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:YearEventData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            YearEventData eventData = AssetDatabase.LoadAssetAtPath<YearEventData>(path);
            if (eventData != null)
            {
                eventDataList.Add(eventData);
            }
        }

        Dictionary<string, Rect> savedPositions = LoadNodePositionsFromFile();
        graphView.PopulateGraphWithSavedPositions(eventDataList, savedPositions);
    }

    private void RefreshGraph()
    {
        if (graphView != null)
        {
            SaveNodePositionsToFile();
        }
        
        eventDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:YearEventData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            YearEventData eventData = AssetDatabase.LoadAssetAtPath<YearEventData>(path);
            if (eventData != null)
            {
                eventDataList.Add(eventData);
            }
        }

        Dictionary<string, Rect> savedPositions = LoadNodePositionsFromFile();
        
        if (savedPositions.Count > 0)
        {
            Debug.Log($"[YearEventGraph] Refreshing with {savedPositions.Count} saved positions");
            graphView.RefreshGraphWithPositions(eventDataList, savedPositions);
        }
        else
        {
            Debug.LogWarning("[YearEventGraph] No saved layout found! Using grid layout");
            graphView.RefreshGraphWithPositions(eventDataList, new Dictionary<string, Rect>());
        }
        
        Debug.Log($"[YearEventGraph] Graph refreshed: {eventDataList.Count} events loaded");
    }

    public void SaveConnections()
    {
        var nodes = graphView.nodes.ToList().Cast<YearEventNode>().ToList();

        foreach (var node in nodes)
        {
            if (node.EventData == null) continue;

            Undo.RecordObject(node.EventData, "Set Next Events");

            var outputPort = node.outputContainer.Q<Port>("NextPort");
            if (outputPort != null)
            {
                var edges = outputPort.connections.ToList();
                node.EventData.nextEventDatas.Clear();
                
                HashSet<YearEventData> uniqueNextEvents = new HashSet<YearEventData>();
                
                foreach (var edge in edges)
                {
                    var targetNode = edge.input.node as YearEventNode;
                    if (targetNode != null && targetNode.EventData != null)
                    {
                        uniqueNextEvents.Add(targetNode.EventData);
                    }
                }
                
                node.EventData.nextEventDatas.AddRange(uniqueNextEvents);
            }

            if (node.EventData.actionDatas != null)
            {
                foreach (var actionData in node.EventData.actionDatas)
                {
                    if (actionData.type == YearActionType.ShowChoicePopup)
                    {
                        var yesPort = node.outputContainer.Q<Port>("YesPort");
                        if (yesPort != null && yesPort.connections.Any())
                        {
                            var yesEdge = yesPort.connections.First();
                            var yesTargetNode = yesEdge.input.node as YearEventNode;
                            if (yesTargetNode != null && yesTargetNode.EventData != null)
                            {
                                actionData.yesNextEvent = yesTargetNode.EventData;
                            }
                        }
                        else
                        {
                            actionData.yesNextEvent = null;
                        }

                        var noPort = node.outputContainer.Q<Port>("NoPort");
                        if (noPort != null && noPort.connections.Any())
                        {
                            var noEdge = noPort.connections.First();
                            var noTargetNode = noEdge.input.node as YearEventNode;
                            if (noTargetNode != null && noTargetNode.EventData != null)
                            {
                                actionData.noNextEvent = noTargetNode.EventData;
                            }
                        }
                        else
                        {
                            actionData.noNextEvent = null;
                        }
                    }
                }
            }

            EditorUtility.SetDirty(node.EventData);
        }

        AssetDatabase.SaveAssets();
        SaveNodePositionsToFile();
        
        Debug.Log("[YearEventGraph] Connections and layout saved to file!");
    }

    #region JSON 파일 저장/로드

    private void SaveNodePositionsToFile()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[YearEventGraph] Skipping save: Currently in Play Mode");
            return;
        }

        if (graphView == null)
        {
            Debug.LogWarning("[YearEventGraph] Cannot save layout: graphView is null");
            return;
        }
        
        var positions = graphView.SaveNodePositions();
        
        if (positions == null || positions.Count == 0)
        {
            Debug.LogWarning("[YearEventGraph] Skipping save: No positions to save");
            return;
        }
        
        var serializable = new SerializablePositionData();
        int validCount = 0;
        int invalidCount = 0;
        
        foreach (var kvp in positions)
        {
            if (kvp.Key != null)
            {
                if (IsValidPosition(kvp.Value))
                {
                    serializable.positions.Add(new NodePosition
                    {
                        eventName = kvp.Key.name,
                        x = kvp.Value.x,
                        y = kvp.Value.y,
                        width = kvp.Value.width,
                        height = kvp.Value.height
                    });
                    validCount++;
                }
                else
                {
                    invalidCount++;
                    //Debug.LogWarning($"[YearEventGraph] Skipping invalid position for '{kvp.Key.name}': " +
                                   //$"x={kvp.Value.x}, y={kvp.Value.y}, w={kvp.Value.width}, h={kvp.Value.height}");
                }
            }
        }
        
        if (serializable.positions.Count == 0)
        {
            Debug.LogWarning($"[YearEventGraph] Skipping save: No valid positions found (invalid: {invalidCount})");
            return;
        }

        if (invalidCount > validCount)
        {
            Debug.LogError($"[YearEventGraph] Aborting save: Too many invalid positions! Valid: {validCount}, Invalid: {invalidCount}");
            return;
        }

        try
        {
            string directory = Path.GetDirectoryName(LAYOUT_FILE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(serializable, true);
            File.WriteAllText(LAYOUT_FILE_PATH, json);
            AssetDatabase.Refresh();
            
            Debug.Log($"[YearEventGraph] Saved {serializable.positions.Count} valid node positions (skipped {invalidCount} invalid)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[YearEventGraph] Failed to save layout: {e.Message}");
        }
    }

    private bool IsValidPosition(Rect rect)
    {
        if (float.IsNaN(rect.x) || float.IsNaN(rect.y) || 
            float.IsNaN(rect.width) || float.IsNaN(rect.height))
        {
            return false;
        }

        if (float.IsInfinity(rect.x) || float.IsInfinity(rect.y) || 
            float.IsInfinity(rect.width) || float.IsInfinity(rect.height))
        {
            return false;
        }

        if (rect.width <= 0 || rect.height <= 0)
        {
            return false;
        }

        if (Mathf.Abs(rect.x) > 100000 || Mathf.Abs(rect.y) > 100000)
        {
            return false;
        }

        return true;
    }

    private Dictionary<string, Rect> LoadNodePositionsFromFile()
    {
        Dictionary<string, Rect> positions = new Dictionary<string, Rect>();
        
        if (!File.Exists(LAYOUT_FILE_PATH))
        {
            Debug.Log($"[YearEventGraph] No layout file found. Will use grid layout.");
            return positions;
        }
        
        try
        {
            string json = File.ReadAllText(LAYOUT_FILE_PATH);
            if (string.IsNullOrEmpty(json))
            {
                return positions;
            }
            
            var serializable = JsonUtility.FromJson<SerializablePositionData>(json);
            if (serializable?.positions != null)
            {
                int validCount = 0;
                int invalidCount = 0;

                foreach (var nodePos in serializable.positions)
                {
                    Rect rect = new Rect(nodePos.x, nodePos.y, nodePos.width, nodePos.height);
                    
                    if (IsValidPosition(rect))
                    {
                        positions[nodePos.eventName] = rect;
                        validCount++;
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogWarning($"[YearEventGraph] Skipping invalid saved position for '{nodePos.eventName}'");
                    }
                }
                
                if (invalidCount > 0)
                {
                    Debug.LogWarning($"[YearEventGraph] Loaded {validCount} valid positions, skipped {invalidCount} invalid positions");
                }
                else
                {
                    Debug.Log($"[YearEventGraph] Loaded {positions.Count} node positions from file");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[YearEventGraph] Failed to load layout: {e.Message}");
        }
        
        return positions;
    }

    #endregion
}

#region Serializable Position Data

[System.Serializable]
public class SerializablePositionData
{
    public List<NodePosition> positions = new List<NodePosition>();
}

[System.Serializable]
public class NodePosition
{
    public string eventName;
    public float x;
    public float y;
    public float width;
    public float height;
}

#endregion

public class YearEventGraphView : GraphView
{
    private YearEventGraphEditorWindow editorWindow;
    private Dictionary<YearEventData, YearEventNode> nodeMap = new Dictionary<YearEventData, YearEventNode>();

    public YearEventGraphView(YearEventGraphEditorWindow window)
    {
        editorWindow = window;

        styleSheets.Add(Resources.Load<StyleSheet>("YearEventGraphStyle"));

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public Dictionary<YearEventData, Rect> SaveNodePositions()
    {
        Dictionary<YearEventData, Rect> positions = new Dictionary<YearEventData, Rect>();
        
        foreach (var kvp in nodeMap)
        {
            if (kvp.Value != null)
            {
                positions[kvp.Key] = kvp.Value.GetPosition();
            }
        }
        
        return positions;
    }

    public void PopulateGraphWithSavedPositions(List<YearEventData> eventDataList, Dictionary<string, Rect> savedPositions)
    {
        nodeMap.Clear();

        var sortedEvents = eventDataList.OrderBy(e => e.name).ToList();

        const float nodeWidth = 250;
        const float nodeHeight = 150;
        const float horizontalSpacing = 300;
        const float verticalSpacing = 200;
        const float startX = 100;
        const float startY = 100;
        const int maxColumns = 4;

        int gridIndex = 0;

        foreach (var eventData in sortedEvents)
        {
            var node = CreateEventNode(eventData);
            
            Rect nodePosition;
            if (savedPositions.ContainsKey(eventData.name))
            {
                nodePosition = savedPositions[eventData.name];
            }
            else
            {
                int row = gridIndex / maxColumns;
                int col = gridIndex % maxColumns;
                
                float x = startX + col * horizontalSpacing;
                float y = startY + row * verticalSpacing;
                
                nodePosition = new Rect(x, y, nodeWidth, nodeHeight);
                gridIndex++;
            }

            node.SetPosition(nodePosition);
            AddElement(node);
            nodeMap[eventData] = node;
        }

        CreateEdgesFromEventData(sortedEvents);
    }

    public void RefreshGraphWithPositions(List<YearEventData> eventDataList, Dictionary<string, Rect> savedPositions)
    {
        foreach (var node in nodes.ToList())
        {
            RemoveElement(node);
        }
        foreach (var edge in edges.ToList())
        {
            RemoveElement(edge);
        }
        nodeMap.Clear();

        var sortedEvents = eventDataList.OrderBy(e => e.name).ToList();

        const float nodeWidth = 250;
        const float nodeHeight = 150;
        const float horizontalSpacing = 300;
        const float verticalSpacing = 200;
        const float startX = 100;
        const float startY = 100;
        const int maxColumns = 4;

        int gridIndex = 0;

        foreach (var eventData in sortedEvents)
        {
            var node = CreateEventNode(eventData);
            
            Rect nodePosition;
            if (savedPositions.ContainsKey(eventData.name))
            {
                nodePosition = savedPositions[eventData.name];
            }
            else
            {
                int row = gridIndex / maxColumns;
                int col = gridIndex % maxColumns;
                
                float x = startX + col * horizontalSpacing;
                float y = startY + row * verticalSpacing;
                
                nodePosition = new Rect(x, y, nodeWidth, nodeHeight);
                gridIndex++;
            }

            node.SetPosition(nodePosition);
            AddElement(node);
            nodeMap[eventData] = node;
        }

        CreateEdgesFromEventData(sortedEvents);
    }

    private void CreateEdgesFromEventData(List<YearEventData> eventDataList)
    {
        foreach (var eventData in eventDataList)
        {
            if (!nodeMap.ContainsKey(eventData)) continue;
            var sourceNode = nodeMap[eventData];

            if (eventData.nextEventDatas != null)
            {
                foreach (var nextEvent in eventData.nextEventDatas)
                {
                    if (nextEvent != null && nodeMap.ContainsKey(nextEvent))
                    {
                        var targetNode = nodeMap[nextEvent];
                        var outputPort = sourceNode.outputContainer.Q<Port>("NextPort");
                        var inputPort = targetNode.inputContainer.Q<Port>();

                        if (outputPort != null && inputPort != null)
                        {
                            var edge = outputPort.ConnectTo(inputPort);
                            AddElement(edge);
                        }
                    }
                }
            }

            if (eventData.actionDatas != null)
            {
                foreach (var actionData in eventData.actionDatas)
                {
                    if (actionData.type == YearActionType.ShowChoicePopup)
                    {
                        if (actionData.yesNextEvent != null && nodeMap.ContainsKey(actionData.yesNextEvent))
                        {
                            var yesPort = sourceNode.outputContainer.Q<Port>("YesPort");
                            var targetNode = nodeMap[actionData.yesNextEvent];
                            var inputPort = targetNode.inputContainer.Q<Port>();

                            if (yesPort != null && inputPort != null)
                            {
                                var edge = yesPort.ConnectTo(inputPort);
                                AddElement(edge);
                            }
                        }

                        if (actionData.noNextEvent != null && nodeMap.ContainsKey(actionData.noNextEvent))
                        {
                            var noPort = sourceNode.outputContainer.Q<Port>("NoPort");
                            var targetNode = nodeMap[actionData.noNextEvent];
                            var inputPort = targetNode.inputContainer.Q<Port>();

                            if (noPort != null && inputPort != null)
                            {
                                var edge = noPort.ConnectTo(inputPort);
                                AddElement(edge);
                            }
                        }
                    }
                }
            }
        }
    }

    private YearEventNode CreateEventNode(YearEventData eventData)
    {
        var node = new YearEventNode(eventData);
        return node;
    }
}

public class YearEventNode : Node
{
    public YearEventData EventData { get; private set; }

    public YearEventNode(YearEventData eventData)
    {
        EventData = eventData;
        title = eventData != null ? eventData.name : "Unknown Event";

        // Style based on initiallyUnlocked
        if (eventData != null && eventData.initiallyUnlocked)
        {
            titleContainer.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
        }

        // Input port (can receive connection from previous event)
        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(YearEventData));
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        // Next port (기본)
        var nextPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(YearEventData));
        nextPort.portName = "Next";
        nextPort.name = "NextPort";
        outputContainer.Add(nextPort);

        // ShowChoicePopup이 있으면 Yes/No 포트 추가
        if (eventData != null && eventData.actionDatas != null)
        {
            bool hasChoiceAction = eventData.actionDatas.Any(a => a.type == YearActionType.ShowChoicePopup);
            
            if (hasChoiceAction)
            {
                // Yes Port
                var yesPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(YearEventData));
                yesPort.portName = "Yes";
                yesPort.name = "YesPort";
                yesPort.portColor = new Color(0.4f, 1f, 0.4f); // 초록색
                outputContainer.Add(yesPort);

                // No Port
                var noPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(YearEventData));
                noPort.portName = "No";
                noPort.name = "NoPort";
                noPort.portColor = new Color(1f, 0.4f, 0.4f); // 빨간색
                outputContainer.Add(noPort);
            }
        }

        // Add info label
        if (eventData != null)
        {
            var infoContainer = new VisualElement();
            infoContainer.style.marginLeft = 5;
            infoContainer.style.marginTop = 5;
            infoContainer.style.marginRight = 5;

            var typeLabel = new Label($"Type: {eventData.executionType}");
            typeLabel.style.fontSize = 10;
            infoContainer.Add(typeLabel);

            var weightLabel = new Label($"Weight: {eventData.weight}");
            weightLabel.style.fontSize = 10;
            infoContainer.Add(weightLabel);

            if (eventData.initiallyUnlocked)
            {
                var unlockedLabel = new Label("★ Initially Unlocked");
                unlockedLabel.style.color = new Color(0.4f, 1f, 0.4f);
                unlockedLabel.style.fontSize = 10;
                infoContainer.Add(unlockedLabel);
            }

            mainContainer.Add(infoContainer);
            AddConditionsInfo(eventData);
            AddFirstActionInfo(eventData);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    private void AddConditionsInfo(YearEventData eventData)
    {
        if (eventData.conditionDatas == null || eventData.conditionDatas.Count == 0) return;

        var conditionsContainer = new VisualElement();
        conditionsContainer.style.marginTop = 5;
        conditionsContainer.style.marginLeft = 5;
        conditionsContainer.style.marginRight = 5;
        conditionsContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
        conditionsContainer.style.borderTopLeftRadius = 4;
        conditionsContainer.style.borderTopRightRadius = 4;
        conditionsContainer.style.borderBottomLeftRadius = 4;
        conditionsContainer.style.borderBottomRightRadius = 4;
        conditionsContainer.style.paddingTop = 3;
        conditionsContainer.style.paddingBottom = 3;
        conditionsContainer.style.paddingLeft = 5;
        conditionsContainer.style.paddingRight = 5;

        var conditionLabel = new Label($"Conditions: {eventData.conditionDatas.Count}");
        conditionLabel.style.fontSize = 9;
        conditionLabel.style.color = new Color(0.7f, 0.7f, 1f);
        conditionsContainer.Add(conditionLabel);

        foreach (var condition in eventData.conditionDatas.Take(2))
        {
            var condText = GetConditionText(condition);
            var label = new Label($"• {condText}");
            label.style.fontSize = 8;
            label.style.color = new Color(0.6f, 0.6f, 0.8f);
            conditionsContainer.Add(label);
        }

        if (eventData.conditionDatas.Count > 2)
        {
            var moreLabel = new Label($"... and {eventData.conditionDatas.Count - 2} more");
            moreLabel.style.fontSize = 8;
            moreLabel.style.color = new Color(0.5f, 0.5f, 0.7f);
            conditionsContainer.Add(moreLabel);
        }

        mainContainer.Add(conditionsContainer);
    }

    private string GetConditionText(YearEventConditionData condition)
    {
        switch (condition.type)
        {
            case YearConditionType.YearGreaterThan:
                return $"Year >= {condition.yearValue}";
            case YearConditionType.YearBetween:
                return $"Year {condition.yearValue}-{condition.yearValueMax}";
            case YearConditionType.GoldGreaterThan:
                return $"Gold >= {condition.goldValue}";
            case YearConditionType.GoldLessThan:
                return $"Gold < {condition.goldValue}";
            case YearConditionType.FoodGreaterThan:
                return $"Food >= {condition.foodValue}";
            case YearConditionType.FoodLessThan:
                return $"Food < {condition.foodValue}";
            case YearConditionType.MemberCountGreaterThan:
                return $"Members >= {condition.memberCount}";
            case YearConditionType.MemberCountLessThan:
                return $"Members < {condition.memberCount}";
            case YearConditionType.EventViewed:
                return $"Viewed: {(condition.requiredEvent != null ? condition.requiredEvent.name : "None")}";
            case YearConditionType.EventNotViewed:
                return $"NotViewed: {(condition.requiredEvent != null ? condition.requiredEvent.name : "None")}";
            default:
                return condition.type.ToString();
        }
    }

    private void AddFirstActionInfo(YearEventData eventData)
    {
        if (eventData.actionDatas == null || eventData.actionDatas.Count == 0) return;

        var firstAction = eventData.actionDatas.FirstOrDefault(a => a.type == YearActionType.ShowEventPopup || a.type == YearActionType.ShowChoicePopup);
        if (firstAction == null) return;

        var actionContainer = new VisualElement();
        actionContainer.style.marginTop = 5;
        actionContainer.style.marginLeft = 5;
        actionContainer.style.marginRight = 5;
        actionContainer.style.marginBottom = 5;
        actionContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        actionContainer.style.borderTopLeftRadius = 4;
        actionContainer.style.borderTopRightRadius = 4;
        actionContainer.style.borderBottomLeftRadius = 4;
        actionContainer.style.borderBottomRightRadius = 4;
        actionContainer.style.paddingTop = 5;
        actionContainer.style.paddingBottom = 5;
        actionContainer.style.paddingLeft = 5;
        actionContainer.style.paddingRight = 5;

        // Action type indicator
        if (firstAction.type == YearActionType.ShowChoicePopup)
        {
            var choiceLabel = new Label("🔀 CHOICE EVENT");
            choiceLabel.style.fontSize = 9;
            choiceLabel.style.color = new Color(1f, 1f, 0.4f);
            choiceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionContainer.Add(choiceLabel);
        }

        // Event text
        string eventText = firstAction.type == YearActionType.ShowChoicePopup 
            ? (firstAction.choiceEventText ?? "") 
            : (firstAction.eventText ?? "");
        
        if (eventText.Length > 60)
        {
            eventText = eventText.Substring(0, 57) + "...";
        }
        var textLabel = new Label(eventText);
        textLabel.style.whiteSpace = WhiteSpace.Normal;
        textLabel.style.fontSize = 9;
        textLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
        actionContainer.Add(textLabel);

        // Rewards (ShowEventPopup만)
        if (firstAction.type == YearActionType.ShowEventPopup && firstAction.rewards != null && firstAction.rewards.Length > 0)
        {
            var rewardsContainer = new VisualElement();
            rewardsContainer.style.flexDirection = FlexDirection.Row;
            rewardsContainer.style.marginTop = 5;
            rewardsContainer.style.flexWrap = Wrap.Wrap;

            foreach (var reward in firstAction.rewards.Take(3))
            {
                if (reward.rewardData == null) continue;

                var rewardBox = new VisualElement();
                rewardBox.style.flexDirection = FlexDirection.Row;
                rewardBox.style.alignItems = Align.Center;
                rewardBox.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                rewardBox.style.borderTopLeftRadius = 3;
                rewardBox.style.borderTopRightRadius = 3;
                rewardBox.style.borderBottomLeftRadius = 3;
                rewardBox.style.borderBottomRightRadius = 3;
                rewardBox.style.paddingLeft = 3;
                rewardBox.style.paddingRight = 3;
                rewardBox.style.paddingTop = 2;
                rewardBox.style.paddingBottom = 2;
                rewardBox.style.marginRight = 3;
                rewardBox.style.marginBottom = 3;

                if (reward.rewardData.sprite != null)
                {
                    var icon = new Image();
                    icon.sprite = reward.rewardData.sprite;
                    icon.style.width = 16;
                    icon.style.height = 16;
                    icon.style.marginRight = 3;
                    rewardBox.Add(icon);
                }

                var amountLabel = new Label($"{(reward.amount > 0 ? "+" : "")}{reward.amount}");
                amountLabel.style.fontSize = 9;
                amountLabel.style.color = reward.amount > 0 ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);
                rewardBox.Add(amountLabel);

                rewardsContainer.Add(rewardBox);
            }

            actionContainer.Add(rewardsContainer);
        }

        mainContainer.Add(actionContainer);
    }
}

public class YearEventToolbar : VisualElement
{
    public YearEventToolbar()
    {
        style.flexDirection = FlexDirection.Row;
        style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.paddingLeft = 5;
        style.paddingRight = 5;
        style.paddingTop = 5;
        style.paddingBottom = 5;
    }
}
