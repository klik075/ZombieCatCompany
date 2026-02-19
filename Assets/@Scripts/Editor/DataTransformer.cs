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
        ParseExcelDataToJson<MemberDataLoader, MemberData>("Member");
        ParseExcelDataToJson<EducationDataLoader, EducationData>("Education");
        ParseExcelDataToJson<GenreDataLoader, GenreData>("Genre");
        ParseExcelDataToJson<ContentDataLoader, ContentData>("Content");
        ParseExcelDataToJson<SynergyDataLoader, SynergyData>("Synergy");
        ParseExcelDataToJson<ModeDataLoader, ModeData>("Mode");
        ParseExcelDataToJson<FenceDataLoader, FenceData>("Fence");
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

        // CSV 파일 읽기 (인코딩 자동 감지)
        string csvContent = ReadCSVWithCorrectEncoding(excelPath);
        List<string[]> rows = ParseCSV(csvContent);
        
        if (rows.Count < 2)
        {
            Debug.LogError("CSV file must have at least a header row and one data row");
            return;
        }

        // 헤더 파싱 (첫 번째 줄)
        string[] headers = rows[0];
        
        // LoaderData의 필드 정보 가져오기
        FieldInfo[] fields = typeof(LoaderData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        
        // 데이터 파싱 (두 번째 줄부터)
        for (int i = 1; i < rows.Count; i++)
        {
            string[] values = rows[i];
            
            if (values.Length != headers.Length)
            {
                Debug.LogWarning($"Row {i + 1} has mismatched column count. Expected {headers.Length}, got {values.Length}. Skipping.");
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
                        Debug.LogError($"Error converting field '{headerName}' at row {i + 1}: {ex.Message}");
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

    // RFC 4180 표준을 따르는 CSV 파서
    private static List<string[]> ParseCSV(string csvContent)
    {
        List<string[]> rows = new List<string[]>();
        List<string> currentRow = new List<string>();
        StringBuilder currentField = new StringBuilder();
        bool inQuotes = false;
        
        for (int i = 0; i < csvContent.Length; i++)
        {
            char c = csvContent[i];
            char nextChar = (i + 1 < csvContent.Length) ? csvContent[i + 1] : '\0';
            
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (nextChar == '"')
                    {
                        // 연속된 따옴표는 이스케이프된 따옴표
                        currentField.Append('"');
                        i++; // 다음 따옴표 건너뛰기
                    }
                    else
                    {
                        // 따옴표 종료
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if (c == '\r' && nextChar == '\n')
                {
                    // Windows 줄바꿈 (\r\n)
                    currentRow.Add(currentField.ToString());
                    rows.Add(currentRow.ToArray());
                    currentRow.Clear();
                    currentField.Clear();
                    i++; // \n 건너뛰기
                }
                else if (c == '\n')
                {
                    // Unix 줄바꿈 (\n)
                    currentRow.Add(currentField.ToString());
                    rows.Add(currentRow.ToArray());
                    currentRow.Clear();
                    currentField.Clear();
                }
                else if (c != '\r')
                {
                    currentField.Append(c);
                }
            }
        }
        
        // 마지막 필드와 행 추가
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(currentRow.ToArray());
        }
        
        return rows;
    }

    // 올바른 인코딩으로 CSV 파일 읽기
    private static string ReadCSVWithCorrectEncoding(string filePath)
    {
        // BOM 확인을 위해 바이트로 읽기
        byte[] bytes = File.ReadAllBytes(filePath);
        
        // UTF-8 BOM 확인
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            Debug.Log("Detected UTF-8 with BOM encoding");
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }
        
        // UTF-8 시도 (BOM 없음)
        try
        {
            string utf8Content = Encoding.UTF8.GetString(bytes);
            // UTF-8로 디코딩 후 재인코딩해서 같으면 유효한 UTF-8
            if (Encoding.UTF8.GetBytes(utf8Content).SequenceEqual(bytes))
            {
                Debug.Log("Detected UTF-8 (no BOM) encoding");
                return utf8Content;
            }
        }
        catch { }
        
        // EUC-KR(CP949) 시도
        try
        {
            Encoding euckr = Encoding.GetEncoding("EUC-KR");
            Debug.Log("Using EUC-KR encoding");
            return euckr.GetString(bytes);
        }
        catch
        {
            // 기본값으로 UTF-8 사용
            Debug.LogWarning("Failed to detect encoding, using UTF-8 as fallback");
            return Encoding.UTF8.GetString(bytes);
        }
    }

#endif
}
