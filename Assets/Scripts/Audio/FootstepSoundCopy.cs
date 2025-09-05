using UnityEngine;
using UnityEditor;
using System.IO;

namespace AudioSystem
{
    /// <summary>
    /// 발자국 사운드를 Resources 폴더로 복사하는 스크립트
    /// </summary>
    public class FootstepSoundCopy : MonoBehaviour
    {
        [ContextMenu("발자국 사운드를 Resources로 복사")]
        public void CopyFootstepSoundsToResources()
        {
            string sourcePath = "Assets/Footsteps - Essentials";
            string targetPath = "Assets/Resources/Footsteps - Essentials";
            
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"발자국 사운드 폴더를 찾을 수 없습니다: {sourcePath}");
                return;
            }
            
            // Resources 폴더 생성
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            
            // 기존 Resources/Footsteps 폴더 삭제
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
            
            // 폴더 복사
            CopyDirectoryRecursive(sourcePath, targetPath);
            
            // AssetDatabase 새로고침
            AssetDatabase.Refresh();
            
            Debug.Log($"발자국 사운드가 {targetPath}로 복사되었습니다!");
        }
        
        private void CopyDirectoryRecursive(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);
            
            // 파일 복사
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                if (file.EndsWith(".wav") || file.EndsWith(".ogg") || file.EndsWith(".mp3"))
                {
                    string fileName = Path.GetFileName(file);
                    string targetFile = Path.Combine(targetDir, fileName);
                    File.Copy(file, targetFile, true);
                }
            }
            
            // 하위 폴더 복사
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                string targetSubDir = Path.Combine(targetDir, subDirName);
                CopyDirectoryRecursive(subDir, targetSubDir);
            }
        }
    }
}
