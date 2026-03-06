using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class DialogueGraphEditorWindow : EditorWindow
{
    private DialogueGraphView graphView;
    private List<DialogueEventData> eventDataList = new List<DialogueEventData>();

    [MenuItem("Tools/Dialogue Event Graph")]
    public static void OpenWindow()
    {
        var window = GetWindow<DialogueGraphEditorWindow>();
        window.titleContent = new GUIContent("Dialogue Event Graph");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        CreateGraphView();
        CreateToolbar();
        LoadAllDialogueEvents();
    }

    private void OnDisable()
    {
        if (graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }
    }

    private void CreateGraphView()
    {
        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Event Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void CreateToolbar()
    {
        var toolbar = new Toolbar();

        var refreshButton = new Button(() => RefreshGraph()) { text = "Refresh" };
        toolbar.Add(refreshButton);

        var saveButton = new Button(() => SaveConnections()) { text = "Save Connections" };
        toolbar.Add(saveButton);

        rootVisualElement.Add(toolbar);
    }

    private void LoadAllDialogueEvents()
    {
        eventDataList.Clear();
        string[] guids = AssetDatabase.FindAssets("t:DialogueEventData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueEventData eventData = AssetDatabase.LoadAssetAtPath<DialogueEventData>(path);
            if (eventData != null)
            {
                eventDataList.Add(eventData);
            }
        }

        graphView.PopulateGraph(eventDataList);
    }

    private void RefreshGraph()
    {
        graphView.ClearGraph();
        LoadAllDialogueEvents();
    }

    public void SaveConnections()
    {
        var nodes = graphView.nodes.ToList().Cast<DialogueEventNode>().ToList();

        foreach (var node in nodes)
        {
            if (node.EventData == null) continue;

            Undo.RecordObject(node.EventData, "Set Next Events");

            // Save Next connections
            var outputPort = node.outputContainer.Q<Port>("NextPort");
            if (outputPort != null)
            {
                var edges = outputPort.connections.ToList();
                node.EventData.nextEventDatas.Clear();
                
                foreach (var edge in edges)
                {
                    var targetNode = edge.input.node as DialogueEventNode;
                    if (targetNode != null && targetNode.EventData != null)
                    {
                        node.EventData.nextEventDatas.Add(targetNode.EventData);
                    }
                }
            }

            // Save Left/Right choice connections
            var firstChoiceAction = node.EventData.actionDatas?.FirstOrDefault(a => a.type == ActionType.ChoiceDialogue);
            if (firstChoiceAction != null)
            {
                var leftPort = node.outputContainer.Q<Port>("LeftChoicePort");
                if (leftPort != null)
                {
                    var leftEdges = leftPort.connections.ToList();
                    if (leftEdges.Count > 0)
                    {
                        var targetNode = leftEdges[0].input.node as DialogueEventNode;
                        firstChoiceAction.leftChoiceEvent = targetNode?.EventData;
                    }
                    else
                    {
                        firstChoiceAction.leftChoiceEvent = null;
                    }
                }

                var rightPort = node.outputContainer.Q<Port>("RightChoicePort");
                if (rightPort != null)
                {
                    var rightEdges = rightPort.connections.ToList();
                    if (rightEdges.Count > 0)
                    {
                        var targetNode = rightEdges[0].input.node as DialogueEventNode;
                        firstChoiceAction.rightChoiceEvent = targetNode?.EventData;
                    }
                    else
                    {
                        firstChoiceAction.rightChoiceEvent = null;
                    }
                }
            }

            EditorUtility.SetDirty(node.EventData);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Dialogue event connections saved!");
    }
}

public class DialogueGraphView : GraphView
{
    private DialogueGraphEditorWindow editorWindow;
    private Dictionary<DialogueEventData, DialogueEventNode> nodeMap = new Dictionary<DialogueEventData, DialogueEventNode>();

    public DialogueGraphView(DialogueGraphEditorWindow window)
    {
        editorWindow = window;

        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyle"));

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

    public void PopulateGraph(List<DialogueEventData> eventDataList)
    {
        nodeMap.Clear();

        // Create nodes
        float xOffset = 0;
        float yOffset = 0;
        int column = 0;

        foreach (var eventData in eventDataList)
        {
            var node = CreateEventNode(eventData);
            node.SetPosition(new Rect(100 + xOffset, 100 + yOffset, 200, 150));
            AddElement(node);
            nodeMap[eventData] = node;

            xOffset += 250;
            column++;
            if (column >= 4)
            {
                column = 0;
                xOffset = 0;
                yOffset += 200;
            }
        }

        // Create edges based on existing connections
        foreach (var eventData in eventDataList)
        {
            if (!nodeMap.ContainsKey(eventData)) continue;
            var sourceNode = nodeMap[eventData];

            // Next events connections
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

            // Left/Right choice connections
            var firstChoiceAction = eventData.actionDatas?.FirstOrDefault(a => a.type == ActionType.ChoiceDialogue);
            if (firstChoiceAction != null)
            {
                // Left choice
                if (firstChoiceAction.leftChoiceEvent != null && nodeMap.ContainsKey(firstChoiceAction.leftChoiceEvent))
                {
                    var targetNode = nodeMap[firstChoiceAction.leftChoiceEvent];
                    var leftPort = sourceNode.outputContainer.Q<Port>("LeftChoicePort");
                    var inputPort = targetNode.inputContainer.Q<Port>();

                    if (leftPort != null && inputPort != null)
                    {
                        var edge = leftPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }

                // Right choice
                if (firstChoiceAction.rightChoiceEvent != null && nodeMap.ContainsKey(firstChoiceAction.rightChoiceEvent))
                {
                    var targetNode = nodeMap[firstChoiceAction.rightChoiceEvent];
                    var rightPort = sourceNode.outputContainer.Q<Port>("RightChoicePort");
                    var inputPort = targetNode.inputContainer.Q<Port>();

                    if (rightPort != null && inputPort != null)
                    {
                        var edge = rightPort.ConnectTo(inputPort);
                        AddElement(edge);
                    }
                }
            }
        }
    }

    private DialogueEventNode CreateEventNode(DialogueEventData eventData)
    {
        var node = new DialogueEventNode(eventData);
        return node;
    }

    public void ClearGraph()
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
    }
}

public class DialogueEventNode : Node
{
    public DialogueEventData EventData { get; private set; }
    private static Dictionary<CharacterId, CharacterData> characterDataCache;

    public DialogueEventNode(DialogueEventData eventData)
    {
        EventData = eventData;
        title = eventData != null ? eventData.EventName : "Unknown Event";

        // Style based on initiallyUnlocked
        if (eventData != null && eventData.initiallyUnlocked)
        {
            titleContainer.style.backgroundColor = new Color(0.2f, 0.6f, 0.2f);
        }

        // Input port (can receive connection from previous event)
        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(DialogueEventData));
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        // Output port (connects to next events - Multi capacity for multiple connections)
        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(DialogueEventData));
        outputPort.portName = "Next";
        outputPort.name = "NextPort";
        outputContainer.Add(outputPort);

        // Check if this event has ChoiceDialogue action
        var hasChoiceDialogue = eventData?.actionDatas?.Any(a => a.type == ActionType.ChoiceDialogue) ?? false;
        if (hasChoiceDialogue)
        {
            // Left choice port
            var leftPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueEventData));
            leftPort.portName = "← Left";
            leftPort.name = "LeftChoicePort";
            leftPort.portColor = new Color(0.4f, 0.8f, 0.4f);
            outputContainer.Add(leftPort);

            // Right choice port
            var rightPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialogueEventData));
            rightPort.portName = "→ Right";
            rightPort.name = "RightChoicePort";
            rightPort.portColor = new Color(0.8f, 0.4f, 0.4f);
            outputContainer.Add(rightPort);
        }

        // Add info label
        if (eventData != null)
        {
            var infoLabel = new Label($"Type: {eventData.executionType}");
            infoLabel.style.marginLeft = 5;
            infoLabel.style.marginTop = 5;
            mainContainer.Add(infoLabel);

            if (eventData.initiallyUnlocked)
            {
                var unlockedLabel = new Label("★ Initially Unlocked");
                unlockedLabel.style.marginLeft = 5;
                unlockedLabel.style.color = new Color(0.4f, 1f, 0.4f);
                mainContainer.Add(unlockedLabel);
            }

            // Find first Dialogue action and display its info
            AddFirstDialogueInfo(eventData);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    private void AddFirstDialogueInfo(DialogueEventData eventData)
    {
        if (eventData.actionDatas == null || eventData.actionDatas.Count == 0) return;

        var firstDialogueAction = eventData.actionDatas.FirstOrDefault(a => a.type == ActionType.Dialogue);
        if (firstDialogueAction == null) return;

        // Create container for dialogue preview
        var dialogueContainer = new VisualElement();
        dialogueContainer.style.marginTop = 5;
        dialogueContainer.style.marginLeft = 5;
        dialogueContainer.style.marginRight = 5;
        dialogueContainer.style.marginBottom = 5;
        dialogueContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
        dialogueContainer.style.borderTopLeftRadius = 4;
        dialogueContainer.style.borderTopRightRadius = 4;
        dialogueContainer.style.borderBottomLeftRadius = 4;
        dialogueContainer.style.borderBottomRightRadius = 4;
        dialogueContainer.style.paddingTop = 5;
        dialogueContainer.style.paddingBottom = 5;
        dialogueContainer.style.paddingLeft = 5;
        dialogueContainer.style.paddingRight = 5;

        // Horizontal container for sprite and text
        var horizontalContainer = new VisualElement();
        horizontalContainer.style.flexDirection = FlexDirection.Row;
        horizontalContainer.style.alignItems = Align.FlexStart;

        // Get character sprite
        var sprite = GetCharacterSprite(firstDialogueAction.characterId, firstDialogueAction.characterState);
        if (sprite != null)
        {
            var spriteImage = new Image();
            spriteImage.sprite = sprite;
            spriteImage.style.width = 48;
            spriteImage.style.height = 48;
            spriteImage.style.marginRight = 8;
            spriteImage.style.borderTopLeftRadius = 4;
            spriteImage.style.borderTopRightRadius = 4;
            spriteImage.style.borderBottomLeftRadius = 4;
            spriteImage.style.borderBottomRightRadius = 4;
            horizontalContainer.Add(spriteImage);
        }

        // Text container
        var textContainer = new VisualElement();
        textContainer.style.flexShrink = 1;
        textContainer.style.flexGrow = 1;

        // Character name label
        var characterName = GetCharacterName(firstDialogueAction.characterId);
        var nameLabel = new Label($"{characterName} ({firstDialogueAction.characterState})");
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.color = new Color(0.8f, 0.8f, 1f);
        nameLabel.style.fontSize = 11;
        textContainer.Add(nameLabel);

        // Dialogue text label (truncated if too long)
        var dialogueText = firstDialogueAction.dialogueText ?? "";
        if (dialogueText.Length > 50)
        {
            dialogueText = dialogueText.Substring(0, 47) + "...";
        }
        var dialogueLabel = new Label(dialogueText);
        dialogueLabel.style.whiteSpace = WhiteSpace.Normal;
        dialogueLabel.style.fontSize = 10;
        dialogueLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
        dialogueLabel.style.marginTop = 2;
        textContainer.Add(dialogueLabel);

        horizontalContainer.Add(textContainer);
        dialogueContainer.Add(horizontalContainer);
        mainContainer.Add(dialogueContainer);
    }

    private static void EnsureCharacterDataCache()
    {
        if (characterDataCache != null) return;

        characterDataCache = new Dictionary<CharacterId, CharacterData>();
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (data != null && !characterDataCache.ContainsKey(data.characterId))
            {
                characterDataCache[data.characterId] = data;
            }
        }
    }

    private Sprite GetCharacterSprite(CharacterId characterId, CharacterState characterState)
    {
        EnsureCharacterDataCache();

        if (!characterDataCache.TryGetValue(characterId, out var characterData)) return null;
        if (characterData.expressions == null) return null;

        var expression = characterData.expressions.FirstOrDefault(e => e.characterState == characterState);
        return expression?.sprite;
    }

    private string GetCharacterName(CharacterId characterId)
    {
        EnsureCharacterDataCache();

        if (characterDataCache.TryGetValue(characterId, out var characterData))
        {
            return characterData.characterName ?? characterId.ToString();
        }
        return characterId.ToString();
    }
}

public class Toolbar : VisualElement
{
    public Toolbar()
    {
        style.flexDirection = FlexDirection.Row;
        style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.paddingLeft = 5;
        style.paddingRight = 5;
        style.paddingTop = 5;
        style.paddingBottom = 5;
    }
}
