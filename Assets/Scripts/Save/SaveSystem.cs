using System;
using System.IO;
using UnityEngine;

// 진행 중인 게임을 파일 하나(persistentDataPath/save.json)에 저장하고 불러온다.
// 용병이 한 명뿐이고 죽으면 새로 시작하는 구조라 슬롯은 하나만 둔다.
// 경로를 받는 Write·TryRead는 테스트에서 임시 경로로 검증하기 위해 공개한다.
public static class SaveSystem
{
    public const string FileName = "save.json";

    public static string DefaultPath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave() => File.Exists(DefaultPath);
    public static bool Save(SaveData data) => Write(DefaultPath, data);
    public static bool TryLoad(out SaveData data) => TryRead(DefaultPath, out data);

    public static void Delete() {
        try {
            if(File.Exists(DefaultPath)) File.Delete(DefaultPath);
        } catch(Exception e) {
            Debug.LogError($"[세이브] 삭제 실패: {e.Message}");
        }
    }

    public static bool Write(string path, SaveData data) {
        if(data == null) return false;

        data.version = SaveData.CurrentVersion;
        data.savedAtUtc = DateTime.UtcNow.ToString("o");

        try {
            string dir = Path.GetDirectoryName(path);
            if(!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // 쓰는 도중에 종료되어도 기존 세이브가 깨지지 않도록 임시 파일에 먼저 쓰고 교체한다
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if(File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
            return true;
        } catch(Exception e) {
            Debug.LogError($"[세이브] 저장 실패 ({path}): {e.Message}");
            return false;
        }
    }

    // 파일이 없거나, 읽을 수 없거나, 버전이 다르면 false
    public static bool TryRead(string path, out SaveData data) {
        data = null;
        if(!File.Exists(path)) return false;

        SaveData loaded;
        try {
            loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        } catch(Exception e) {
            Debug.LogWarning($"[세이브] 읽기 실패 ({path}): {e.Message}");
            return false;
        }

        // JsonUtility는 빠진 객체 필드를 기본값 객체로 채우기도 하므로 id까지 확인한다
        if(loaded == null || loaded.mercenary == null || string.IsNullOrEmpty(loaded.mercenary.id)) {
            Debug.LogWarning($"[세이브] 용병 정보가 없는 세이브입니다 ({path})");
            return false;
        }
        if(loaded.version != SaveData.CurrentVersion) {
            Debug.LogWarning($"[세이브] 세이브 버전 {loaded.version}은 지원하지 않습니다 (현재 {SaveData.CurrentVersion})");
            return false;
        }

        data = loaded;
        return true;
    }
}
