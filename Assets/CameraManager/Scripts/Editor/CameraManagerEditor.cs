using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework
{
    [CustomEditor(typeof(CameraManager))]
    public class CameraControllerEditor : Editor
    {
        private CameraManager    cc;
        private SerializedObject    viObject;

        private SerializedProperty  rigXProp;
        private SerializedProperty  rigYProp;
        private SerializedProperty  rigZProp;

        private SerializedProperty  rigOffsetXProp;
        private SerializedProperty  rigOffsetYProp;
        private SerializedProperty  rigOffsetZProp;

        private SerializedProperty  defaultFOVProp;
        //private SerializedProperty  fovProp;

        private SerializedProperty  defaultPitchProp;
        //private SerializedProperty  pitchProp;
        private SerializedProperty  minPitchProp;
        private SerializedProperty  maxPitchProp;

        private SerializedProperty  defaultYawProp;
        //private SerializedProperty  yawProp;

        private SerializedProperty  defaultDistanceProp;
        //private SerializedProperty  distanceProp;
        private SerializedProperty  minDistanceProp;
        private SerializedProperty  maxDistanceProp;

        private Transform               viewTarget;
        private CameraViewInfo          viewInfo;
        private CameraEffectInfo     effectProfile;
        private CameraViewInfoCollection   viewInfoProfile;
        
        private void OnEnable()
        {
            cc = (CameraManager)target;

            GetViewInfoProperties();

            cc.GetEffectProfile(out effectProfile);
            if( effectProfile == null )
            {
                effectProfile = AssetDatabase.LoadAssetAtPath<CameraEffectInfo>(cc.effectProfileAssetPath);
            }

            cc.GetViewInfoProfile(out viewInfoProfile);
            if( viewInfoProfile == null )
            {
                viewInfoProfile = AssetDatabase.LoadAssetAtPath<CameraViewInfoCollection>(cc.viewInfoProfileAssetPath);
            }
        }

        private void GetViewInfoProperties()
        {
            cc.GetViewInfo(out viewTarget, out viewInfo);
            if (viewInfo != null)
            {
                viObject = new SerializedObject(viewInfo);

                rigXProp = viObject.FindProperty("rig.x");
                rigYProp = viObject.FindProperty("rig.y");
                rigZProp = viObject.FindProperty("rig.z");

                rigOffsetXProp = viObject.FindProperty("rigOffset.x");
                rigOffsetYProp = viObject.FindProperty("rigOffset.y");
                rigOffsetZProp = viObject.FindProperty("rigOffset.z");

                defaultFOVProp = viObject.FindProperty("defaultFOV");
                //fovProp = viObject.FindProperty("fov");

                defaultPitchProp = viObject.FindProperty("defaultPitch");
                //pitchProp = viObject.FindProperty("pitch");
                minPitchProp = viObject.FindProperty("minPitch");
                maxPitchProp = viObject.FindProperty("maxPitch");

                defaultYawProp = viObject.FindProperty("defaultYaw");
                //yawProp = viObject.FindProperty("yaw");
                defaultYawProp = viObject.FindProperty("defaultYaw");

                defaultDistanceProp = viObject.FindProperty("defaultDistance");
                //distanceProp = viObject.FindProperty("distance");
                minDistanceProp = viObject.FindProperty("minDistance");
                maxDistanceProp = viObject.FindProperty("maxDistance");
            }
            else
            {
                viewInfo = AssetDatabase.LoadAssetAtPath<CameraViewInfo>(cc.assetPath);
            }
        }

        public override void OnInspectorGUI()
        {
            //////// 编辑全局参数
            GUILayout.Space(10);
            GUILayout.Label("编辑全局参数（灵敏、平滑系数）", EGUIStyles.TitleTextStyle);

            DrawDefaultInspector();

            //////// 编辑相机位
            GUILayout.Space(30);
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            {
                GUILayout.Label("编辑相机位参数", EGUIStyles.TitleTextStyle);


                ////// 编辑ViewTarget & CameraViewInfo
                viewTarget = (Transform)EditorGUILayout.ObjectField("View Target", viewTarget, typeof(Transform), true, GUILayout.ExpandWidth(true));
                viewInfo = (CameraViewInfo)EditorGUILayout.ObjectField("Camera ViewInfo", viewInfo, typeof(CameraViewInfo), true, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("资源路径：       " + cc.assetPath, GUILayout.ExpandWidth(true));
                if (viewInfo != null)
                {
                    // 记录资源地址，方便执行“Save”
                    string path = AssetDatabase.GetAssetPath(viewInfo);
                    if (!string.IsNullOrEmpty(path))
                    {
                        cc.assetPath = path;
                    }
                }

                ////// GO
                EditorGUILayout.Separator();
                if (GUILayout.Button("GO", GUILayout.Height(30)))
                {
                    if (viewInfo != null)
                    {
                        GotoViewTarget(viewTarget, viewInfo, 0.3f);
                    }
                }

                ////// SAVE & SAVE AS
                EditorGUI.BeginDisabledGroup(!CanSave(viewInfo));
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save", GUILayout.Width(200)))
                    {
                        cc.SaveToDefault(viewTarget, viewInfo);
                        AssetDatabase.CreateAsset(viewInfo, cc.assetPath);
                        AssetDatabase.Refresh();
                    }
                    if (GUILayout.Button("Save as...", GUILayout.Width(200)))
                    {
                        string savePath = EditorUtility.SaveFilePanel("", cc.assetPath, "CameraViewInfo", "asset");
                        if (!string.IsNullOrEmpty(savePath))
                        {
                            savePath = savePath.Substring(Application.dataPath.Length - 6);

                            cc.SaveToDefault(viewTarget, viewInfo);
                            AssetDatabase.CreateAsset(viewInfo, savePath);
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
                
                GUILayout.Space(10);
                GUILayout.Label("当前相机位参数", EGUIStyles.Label2);
                if (viObject != null)
                {
                    viObject.Update();

                    EditorGUILayout.LabelField("rig", GUILayout.ExpandWidth(false));
                    {
                        ++EditorGUI.indentLevel;
                        //EditorGUILayout.DelayedFloatField(rigXProp, GUILayout.ExpandWidth(true));
                        //EditorGUILayout.DelayedFloatField(rigYProp, GUILayout.ExpandWidth(true));
                        //EditorGUILayout.DelayedFloatField(rigZProp, GUILayout.ExpandWidth(true));
                        --EditorGUI.indentLevel;
                    }

                    EditorGUILayout.LabelField("rigOffset", GUILayout.ExpandWidth(false));
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.DelayedFloatField(rigOffsetXProp, GUILayout.ExpandWidth(true));
                        EditorGUILayout.DelayedFloatField(rigOffsetYProp, GUILayout.ExpandWidth(true));
                        EditorGUILayout.DelayedFloatField(rigOffsetZProp, GUILayout.ExpandWidth(true));
                        --EditorGUI.indentLevel;
                    }

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultFOVProp, 5, 90, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    viewInfo.fov = EditorGUILayout.Slider("fov", viewInfo.fov, 5, 90, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultPitchProp, minPitchProp.floatValue, maxPitchProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    viewInfo.pitch = EditorGUILayout.Slider("pitch", viewInfo.pitch, minPitchProp.floatValue, maxPitchProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(minPitchProp, -80, 0, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(maxPitchProp, 0, 80, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultYawProp, -180, 180, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    viewInfo.yaw = EditorGUILayout.Slider("yaw", viewInfo.yaw, -180, 180, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultDistanceProp, minDistanceProp.floatValue, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    viewInfo.distance = EditorGUILayout.Slider("distance", viewInfo.distance, minDistanceProp.floatValue, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(minDistanceProp, 0.2f, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(maxDistanceProp, minDistanceProp.floatValue, 80, GUILayout.ExpandWidth(true));

                    viObject.ApplyModifiedProperties();
                }

            }
            EditorGUI.EndDisabledGroup();
            
            
            ////// 编辑CameraViewInfoProfile
            GUILayout.Space(20);
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            {
                GUILayout.Label("编辑相机位组参数", EGUIStyles.TitleTextStyle);

                viewInfoProfile = (CameraViewInfoCollection)EditorGUILayout.ObjectField("Camera ViewInfo Profile", viewInfoProfile, typeof(CameraViewInfoCollection), true, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("资源路径：       " + cc.viewInfoProfileAssetPath, GUILayout.ExpandWidth(true));
                if (viewInfoProfile != null)
                {
                    // 记录资源地址，方便执行“Save”
                    string path = AssetDatabase.GetAssetPath(viewInfoProfile);
                    if (!string.IsNullOrEmpty(path))
                    {
                        cc.viewInfoProfileAssetPath = path;
                    }
                }

                EditorGUI.BeginDisabledGroup(viewInfo == null || viewInfoProfile == null || CanSave(viewInfo));
                {
                    if (GUILayout.Button("Save to FreeView", GUILayout.ExpandWidth(true)))
                    {
                        CameraViewInfo vi = AssetDatabase.LoadAssetAtPath<CameraViewInfo>(cc.assetPath);
                        if (vi != null)
                        {
                            //viewInfoProfile.freeView = vi;
                            EditorUtility.SetDirty(viewInfoProfile);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    if (GUILayout.Button("Save to CloseupView", GUILayout.ExpandWidth(true)))
                    {
                        CameraViewInfo vi = AssetDatabase.LoadAssetAtPath<CameraViewInfo>(cc.assetPath);
                        if (vi != null)
                        {
                            //viewInfoProfile.closeupView = vi;
                            EditorUtility.SetDirty(viewInfoProfile);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    if (GUILayout.Button("Save to ExtremeCloseupView", GUILayout.ExpandWidth(true)))
                    {
                        CameraViewInfo vi = AssetDatabase.LoadAssetAtPath<CameraViewInfo>(cc.assetPath);
                        if (vi != null)
                        {
                            //viewInfoProfile.extremeCloseupView = vi;
                            EditorUtility.SetDirty(viewInfoProfile);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();




            //////// 编辑相机震屏
            GUILayout.Space(30);
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            {
                GUILayout.Label("编辑相机震屏", EGUIStyles.TitleTextStyle);

                ////// CameraEffectProfile
                effectProfile = (CameraEffectInfo)EditorGUILayout.ObjectField("CameraEffect Profile", effectProfile, typeof(CameraEffectInfo), true, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("资源路径：       " + cc.effectProfileAssetPath, GUILayout.ExpandWidth(true));
                if (effectProfile != null)
                {
                    // 记录资源地址，方便执行“Save”
                    string path = AssetDatabase.GetAssetPath(effectProfile);
                    if (!string.IsNullOrEmpty(path))
                    {
                        cc.effectProfileAssetPath = path;
                    }
                }

                ////// GO
                EditorGUILayout.Separator();
                if (GUILayout.Button("GO", GUILayout.Height(30)))
                {
                    if (effectProfile != null)
                    {
                        CameraEffectInfo ep;
                        cc.GetEffectProfile(out ep);

                        if( ep == null || !CanSave(ep) )
                        {
                            ep = Object.Instantiate<CameraEffectInfo>(effectProfile);
                        }
                        effectProfile = ep;

                        cc.PlayCameraEffect(effectProfile);
                    }
                }

                ////// SAVE & SAVE AS
                EditorGUI.BeginDisabledGroup(!CanSave(effectProfile));
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save", GUILayout.Width(200)))
                    {
                        AssetDatabase.CreateAsset(effectProfile, cc.effectProfileAssetPath);
                        AssetDatabase.Refresh();
                    }
                    if (GUILayout.Button("Save as...", GUILayout.Width(200)))
                    {
                        string savePath = EditorUtility.SaveFilePanel("", cc.effectProfileAssetPath, "CameraEffectProfile", "asset");
                        if (!string.IsNullOrEmpty(savePath))
                        {
                            savePath = savePath.Substring(Application.dataPath.Length - 6);
                            AssetDatabase.CreateAsset(effectProfile, savePath);
                            AssetDatabase.Refresh();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.Space(10);
                GUILayout.Label("当前相机位参数", EGUIStyles.Label2);
                if( effectProfile != null )
                {
                    Editor editor = CreateEditor(effectProfile);
                    editor.OnInspectorGUI();
                }

            }
            EditorGUI.EndDisabledGroup();
        }

        // 判断资源是否可以保存，仅实例化资源可以被保存
        private bool CanSave(Object asset)
        {
            if (asset == null)
                return false;

            return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset));
        }

        private void GotoViewTarget(Transform viewTarget, CameraViewInfo viewInfo, float smoothTime)
        {
            cc.EditorSetViewTarget(viewTarget, viewInfo, smoothTime);

            GetViewInfoProperties();
        }
    }
}