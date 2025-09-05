using UnityEngine;
using UnityEditor;
using System.IO;

namespace AudioSystem
{
    /// <summary>
    /// 발자국 사운드를 Resources 폴더로 설정하는 에디터 스크립트
    /// </summary>
    public class FootstepSoundSetup : EditorWindow
    {
        [MenuItem("Tools/Audio System/Setup Footstep Sounds")]
        public static void SetupFootstepSounds()
        {
            string sourcePath = "Assets/Footsteps - Essentials";
            string targetPath = "Assets/Resources/Footsteps - Essentials";
            
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"발자국 사운드 폴더를 찾을 수 없습니다: {sourcePath}");
                return;
            }
            
            // Resources 폴더에 발자국 사운드 복사
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
            
            CopyDirectory(sourcePath, targetPath);
            
            // 메타 파일들도 복사
            CopyMetaFiles(sourcePath, targetPath);
            
            AssetDatabase.Refresh();
            
            Debug.Log("발자국 사운드가 Resources 폴더로 복사되었습니다!");
        }
        
        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }
            
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, subDirName);
                CopyDirectory(subDir, targetSubDir);
            }
        }
        
        private static void CopyMetaFiles(string sourceDir, string targetDir)
        {
            foreach (string file in Directory.GetFiles(sourceDir, "*.meta"))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }
            
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, subDirName);
                CopyMetaFiles(subDir, targetSubDir);
            }
        }
    }
}
