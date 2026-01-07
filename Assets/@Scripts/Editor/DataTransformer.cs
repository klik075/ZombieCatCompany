using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR

    [MenuItem("Tools/ParseExcel %#K")] // Ctrl+Shift+K
    public static void ParseExcelDataToJson()
    {
        ParseExcelDataToJson<ItemDataLoader, ItemData>("Item");
        ParseExcelDataToJson<TextDataLoader, TextData>("Text");
    }

    private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new()
    {
        // CSV 파일 경로
        string excelPath = $"{Application.dataPath}/@ExcelData/{filename}Data.csv";
        
        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excel file not found: {excelPath}");
            return;
        }

        // Loader 객체 생성
        Loader loader = new Loader();
        
        // Loader의 리스트 필드 찾기 (예: items)
        FieldInfo listField = typeof(Loader).GetFields(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType.IsGenericType && 
                               f.FieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                               f.FieldType.GetGenericArguments()[0] == typeof(LoaderData));

        if (listField == null)
        {
            Debug.LogError($"List<{typeof(LoaderData).Name}> field not found in {typeof(Loader).Name}");
            return;
        }

        // 리스트 인스턴스 가져오기
        var dataList = listField.GetValue(loader) as System.Collections.IList;
        if (dataList == null)
        {
            Debug.LogError("Failed to get list instance");
            return;
        }

        // CSV 파일 읽기
        string[] lines = File.ReadAllLines(excelPath, Encoding.UTF8);
        
        if (lines.Length < 2)
        {
            Debug.LogError("CSV file must have at least a header row and one data row");
            return;
        }

        // 헤더 파싱 (첫 번째 줄)
        string[] headers = lines[0].Split(',');
        
        // LoaderData의 필드 정보 가져오기
        FieldInfo[] fields = typeof(LoaderData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        // 데이터 파싱 (두 번째 줄부터)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            
            if (values.Length != headers.Length)
            {
                Debug.LogWarning($"Line {i + 1} has mismatched column count. Skipping.");
                continue;
            }

            // LoaderData 인스턴스 생성
            LoaderData data = Activator.CreateInstance<LoaderData>();

            // 각 필드에 값 할당
            for (int j = 0; j < headers.Length; j++)
            {
                string headerName = headers[j].Trim();
                string value = values[j].Trim();

                // 헤더 이름과 일치하는 필드 찾기
                FieldInfo field = fields.FirstOrDefault(f => f.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase));
                
                if (field != null)
                {
                    try
                    {
                        // 타입에 맞게 변환하여 할당
                        object convertedValue = ConvertValue(value, field.FieldType);
                        field.SetValue(data, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error converting field '{headerName}' at line {i + 1}: {ex.Message}");
                    }
                }
            }

            dataList.Add(data);
        }

        // JSON으로 변환
        string json = JsonConvert.SerializeObject(loader, Formatting.Indented);

        // JSON 파일 저장 경로
        string jsonPath = $"{Application.dataPath}/Resources/Data/JsonData/{filename}Data.json";
        
        // 디렉토리 확인 및 생성
        string directory = Path.GetDirectoryName(jsonPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // JSON 파일 저장
        File.WriteAllText(jsonPath, json, Encoding.UTF8);

        AssetDatabase.Refresh();
        
        Debug.Log($"Successfully converted {filename}Data.csv to JSON. {dataList.Count} items parsed.");
        Debug.Log($"Saved to: {jsonPath}");
    }

    private static object ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        if (targetType == typeof(int))
            return int.Parse(value);
        else if (targetType == typeof(float))
            return float.Parse(value);
        else if (targetType == typeof(double))
            return double.Parse(value);
        else if (targetType == typeof(bool))
            return bool.Parse(value);
        else if (targetType == typeof(string))
            return value;
        else if (targetType.IsEnum)
            return Enum.Parse(targetType, value);
        else
            return Convert.ChangeType(value, targetType);
    }

#endif
}
