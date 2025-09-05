using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.Collections.Generic;
using System.IO;

namespace AudioSystem
{
    /// <summary>
    /// 어드레서블 발자국 사운드 설정 도구
    /// </summary>
    public class AddressableFootstepSetup : EditorWindow
    {
        private string sourcePath = "Assets/Footsteps - Essentials";
        private string groupName = "Footstep Sounds";
        private bool createLabels = true;
        private bool organizeByGroundType = true;
        
        [MenuItem("Tools/Audio System/Setup Addressable Footstep Sounds")]
        public static void ShowWindow()
        {
            GetWindow<AddressableFootstepSetup>("Addressable Footstep Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("어드레서블 발자국 사운드 설정", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            sourcePath = EditorGUILayout.TextField("소스 경로:", sourcePath);
            groupName = EditorGUILayout.TextField("그룹 이름:", groupName);
            createLabels = EditorGUILayout.Toggle("레이블 생성:", createLabels);
            organizeByGroundType = EditorGUILayout.Toggle("지형별 정리:", organizeByGroundType);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("어드레서블 설정 시작"))
            {
                SetupAddressableFootstepSounds();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("레이블만 생성"))
            {
                CreateFootstepLabels();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("기존 설정 확인"))
            {
                CheckExistingSetup();
            }
        }
        
        private void SetupAddressableFootstepSounds()
        {
            if (!Directory.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("오류", $"소스 경로를 찾을 수 없습니다: {sourcePath}", "확인");
                return;
            }
            
            // 어드레서블 설정 가져오기
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("오류", "어드레서블 설정을 찾을 수 없습니다. Window > Asset Management > Addressables > Groups를 먼저 열어주세요.", "확인");
                return;
            }
            
            // 그룹 생성 또는 찾기
            var group = FindOrCreateGroup(settings, groupName);
            
            // 발자국 사운드 파일들 찾기
            var footstepFiles = FindFootstepFiles();
            
            int processedCount = 0;
            int totalCount = footstepFiles.Count;
            
            foreach (var file in footstepFiles)
            {
                // 어드레서블에 추가
                var entry = settings.CreateOrMoveEntry(file.guid, group);
                
                // 레이블 생성 및 할당
                if (createLabels)
                {
                    string label = GenerateLabel(file);
                    entry.labels.Add(label);
                }
                
                processedCount++;
                EditorUtility.DisplayProgressBar("어드레서블 설정", $"처리 중... {processedCount}/{totalCount}", (float)processedCount / totalCount);
            }
            
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("완료", $"어드레서블 설정이 완료되었습니다!\n처리된 파일: {processedCount}개", "확인");
        }
        
        private AddressableAssetGroup FindOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, false, null);
                Debug.Log($"AddressableFootstepSetup: {groupName} 그룹이 생성되었습니다.");
            }
            return group;
        }
        
        private List<FootstepFileInfo> FindFootstepFiles()
        {
            var files = new List<FootstepFileInfo>();
            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { sourcePath });
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                var directory = Path.GetDirectoryName(path);
                
                files.Add(new FootstepFileInfo
                {
                    guid = guid,
                    path = path,
                    fileName = fileName,
                    directory = directory
                });
            }
            
            return files;
        }
        
        private string GenerateLabel(FootstepFileInfo file)
        {
            string label = "footstep_";
            
            // 지형 타입 추출
            if (file.directory.Contains("Grass"))
                label += "grass_";
            else if (file.directory.Contains("DirtyGround"))
                label += "dirt_";
            else if (file.directory.Contains("Gravel"))
                label += "gravel_";
            else if (file.directory.Contains("Metal"))
                label += "metal_";
            else if (file.directory.Contains("Wood"))
                label += "wood_";
            else if (file.directory.Contains("Rock"))
                label += "stone_";
            else if (file.directory.Contains("Sand"))
                label += "sand_";
            else if (file.directory.Contains("Snow"))
                label += "snow_";
            else if (file.directory.Contains("Water"))
                label += "water_";
            else if (file.directory.Contains("Tile"))
                label += "tile_";
            else if (file.directory.Contains("Mud"))
                label += "mud_";
            else if (file.directory.Contains("Leaves"))
                label += "leaves_";
            
            // 액션 타입 추출
            if (file.directory.Contains("Walk"))
                label += "walk";
            else if (file.directory.Contains("Run"))
                label += "run";
            else if (file.directory.Contains("Jump"))
                label += "jump";
            else if (file.directory.Contains("Land"))
                label += "land";
            
            return label;
        }
        
        private void CreateFootstepLabels()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("오류", "어드레서블 설정을 찾을 수 없습니다.", "확인");
                return;
            }
            
            var labels = new[]
            {
                "footstep_grass_walk", "footstep_grass_run", "footstep_grass_jump",
                "footstep_dirt_walk", "footstep_dirt_run", "footstep_dirt_land",
                "footstep_gravel_walk", "footstep_gravel_run", "footstep_gravel_jump",
                "footstep_metal_walk", "footstep_metal_run", "footstep_metal_jump",
                "footstep_wood_walk", "footstep_wood_run", "footstep_wood_jump",
                "footstep_stone_walk", "footstep_stone_run", "footstep_stone_jump",
                "footstep_sand_walk", "footstep_sand_run", "footstep_sand_jump",
                "footstep_snow_walk", "footstep_snow_run", "footstep_snow_jump",
                "footstep_water_walk", "footstep_water_run", "footstep_water_jump",
                "footstep_tile_walk", "footstep_tile_run", "footstep_tile_jump",
                "footstep_mud_walk", "footstep_mud_run", "footstep_mud_jump",
                "footstep_leaves_walk", "footstep_leaves_run", "footstep_leaves_jump"
            };
            
            foreach (var label in labels)
            {
                if (!settings.GetLabels().Contains(label))
                {
                    settings.AddLabel(label);
                }
            }
            
            EditorUtility.DisplayDialog("완료", "발자국 사운드 레이블이 생성되었습니다!", "확인");
        }
        
        private void CheckExistingSetup()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("오류", "어드레서블 설정을 찾을 수 없습니다.", "확인");
                return;
            }
            
            var groups = settings.groups;
            var labels = settings.GetLabels();
            
            string message = "=== 어드레서블 설정 현황 ===\n\n";
            message += $"그룹 수: {groups.Count}\n";
            message += $"레이블 수: {labels.Count}\n\n";
            
            message += "발자국 관련 그룹:\n";
            foreach (var group in groups)
            {
                if (group.name.ToLower().Contains("footstep") || group.name.ToLower().Contains("sound"))
                {
                    message += $"- {group.name} ({group.entries.Count}개 항목)\n";
                }
            }
            
            message += "\n발자국 관련 레이블:\n";
            foreach (var label in labels)
            {
                if (label.StartsWith("footstep_"))
                {
                    message += $"- {label}\n";
                }
            }
            
            EditorUtility.DisplayDialog("어드레서블 설정 현황", message, "확인");
        }
        
        private class FootstepFileInfo
        {
            public string guid;
            public string path;
            public string fileName;
            public string directory;
        }
    }
}
