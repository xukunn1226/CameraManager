using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework
{
    [CustomEditor(typeof(CameraManager))]
    public class CameraManagerEditor : Editor
    {
        private CameraManager               cc;
        private SerializedObject            m_viObject;

        private SerializedProperty          rigOffsetXProp;
        private SerializedProperty          rigOffsetYProp;
        private SerializedProperty          rigOffsetZProp;
        //private SerializedProperty          rigOffsetXWhenAimProp;
        //private SerializedProperty          rigOffsetYWhenAimProp;
        //private SerializedProperty          rigOffsetZWhenAimProp;

        private SerializedProperty          defaultFOVProp;
        //private SerializedProperty        fovProp;

        private SerializedProperty          defaultPitchProp;
        //private SerializedProperty        pitchProp;
        private SerializedProperty          minPitchProp;
        private SerializedProperty          maxPitchProp;

        private SerializedProperty          defaultYawProp;
        //private SerializedProperty        yawProp;

        private SerializedProperty          defaultDistanceProp;
        //private SerializedProperty        distanceProp;
        private SerializedProperty          minDistanceProp;
        private SerializedProperty          maxDistanceProp;

        private Transform                   m_ViewTarget;
        private CameraViewInfo              m_PendingViewInfo;
        private CameraViewInfo              m_SrcViewInfo;
        private string                      m_ViewInfoAssetPath;
        
        private CameraEffectInfo            effectProfile;

        private void OnEnable()
        {
            cc = (CameraManager)target;

            RefreshViewInfoProperties();

            cc.GetEffectProfile(out effectProfile);
            if( effectProfile == null )
            {
                effectProfile = AssetDatabase.LoadAssetAtPath<CameraEffectInfo>(cc.effectProfileAssetPath);
            }
        }

        private void RefreshViewInfoProperties()
        {
            cc.GetViewInfo(out m_ViewTarget, out m_PendingViewInfo, out m_SrcViewInfo);
            if (m_PendingViewInfo != null)
            {
                m_viObject = new SerializedObject(m_PendingViewInfo);

                rigOffsetXProp = m_viObject.FindProperty("rigOffset.x");
                rigOffsetYProp = m_viObject.FindProperty("rigOffset.y");
                rigOffsetZProp = m_viObject.FindProperty("rigOffset.z");
                //rigOffsetXWhenAimProp = m_viObject.FindProperty("rigOffsetWhenAim.x");
                //rigOffsetYWhenAimProp = m_viObject.FindProperty("rigOffsetWhenAim.y");
                //rigOffsetZWhenAimProp = m_viObject.FindProperty("rigOffsetWhenAim.z");

                defaultFOVProp = m_viObject.FindProperty("defaultFOV");
                //fovProp = viObject.FindProperty("fov");

                defaultPitchProp = m_viObject.FindProperty("defaultPitch");
                //pitchProp = viObject.FindProperty("pitch");
                minPitchProp = m_viObject.FindProperty("minPitch");
                maxPitchProp = m_viObject.FindProperty("maxPitch");

                defaultYawProp = m_viObject.FindProperty("defaultYaw");
                //yawProp = viObject.FindProperty("yaw");

                defaultDistanceProp = m_viObject.FindProperty("defaultDistance");
                //distanceProp = viObject.FindProperty("distance");
                minDistanceProp = m_viObject.FindProperty("minDistance");
                maxDistanceProp = m_viObject.FindProperty("maxDistance");
            }

            m_ViewInfoAssetPath = AssetDatabase.GetAssetPath(m_SrcViewInfo);
        }

        public override void OnInspectorGUI()
        {
            //////// 编辑全局参数
            GUILayout.Space(10);
            GUILayout.Label("编辑相机参数（灵敏、平滑系数）", EGUIStyles.TitleTextStyle);

            DrawDefaultInspector();

            //////// 编辑相机位
            GUILayout.Space(30);
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            {
                GUILayout.Label("编辑相机位参数", EGUIStyles.TitleTextStyle);


                ////// 编辑ViewTarget & CameraViewInfo
                EditorGUILayout.LabelField("资源路径：       " + m_ViewInfoAssetPath, GUILayout.ExpandWidth(true));

                EditorGUILayout.BeginHorizontal();
                {
                    cc.curCharView = (CameraViewInfoCollection.CharacterView)EditorGUILayout.EnumPopup("CharView", cc.curCharView, GUILayout.ExpandWidth(true));

                    //Color preColor = GUI.color;
                    //GUI.color = cc.m_isAiming ? Color.green : Color.red;
                    //if (GUILayout.Button("Aiming"))
                    //{
                    //    cc.m_isAiming = !cc.m_isAiming;
                    //}
                    //GUI.color = preColor;
                }
                EditorGUILayout.EndHorizontal();

                ////// GO
                EditorGUILayout.Separator();
                if (GUILayout.Button("GO", GUILayout.Height(30)))
                {
                    EditorSetCharacterView(cc.curCharView, cc.m_isAiming, 0.2f);
                }

                ////// SAVE & SAVE AS
                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_ViewInfoAssetPath));
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save", GUILayout.Height(20)))
                    {
                        cc.SaveToDefault(m_ViewTarget, m_PendingViewInfo, m_SrcViewInfo);
                        AssetDatabase.Refresh();
                    }
                    //if (GUILayout.Button("Save as...", GUILayout.Width(200)))
                    //{
                    //    //string savePath = EditorUtility.SaveFilePanel("", cc.assetPath, "CameraViewInfo", "asset");
                    //    //if (!string.IsNullOrEmpty(savePath))
                    //    //{
                    //    //    savePath = savePath.Substring(Application.dataPath.Length - 6);

                    //    //    cc.SaveToDefault(m_ViewTarget, m_PendingViewInfo);
                    //    //    AssetDatabase.CreateAsset(m_PendingViewInfo, savePath);
                    //    //    AssetDatabase.Refresh();
                    //    //}
                    //}
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
                
                GUILayout.Space(10);
                GUILayout.Label("当前相机位参数", EGUIStyles.Label2);
                if (m_viObject != null)
                {
                    m_viObject.Update();

                    EditorGUILayout.LabelField("rigOffset", GUILayout.ExpandWidth(false));
                    {
                        ++EditorGUI.indentLevel;
                        EditorGUILayout.DelayedFloatField(rigOffsetXProp, GUILayout.ExpandWidth(true));
                        EditorGUILayout.DelayedFloatField(rigOffsetYProp, GUILayout.ExpandWidth(true));
                        EditorGUILayout.DelayedFloatField(rigOffsetZProp, GUILayout.ExpandWidth(true));
                        --EditorGUI.indentLevel;
                    }
                    //EditorGUILayout.LabelField("rigOffsetWhenAim", GUILayout.ExpandWidth(false));
                    //{
                    //    ++EditorGUI.indentLevel;
                    //    EditorGUILayout.DelayedFloatField(rigOffsetXWhenAimProp, GUILayout.ExpandWidth(true));
                    //    EditorGUILayout.DelayedFloatField(rigOffsetYWhenAimProp, GUILayout.ExpandWidth(true));
                    //    EditorGUILayout.DelayedFloatField(rigOffsetZWhenAimProp, GUILayout.ExpandWidth(true));
                    //    --EditorGUI.indentLevel;
                    //}

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultFOVProp, 5, 90, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    m_PendingViewInfo.fov = EditorGUILayout.Slider("fov", m_PendingViewInfo.fov, 5, 90, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultPitchProp, minPitchProp.floatValue, maxPitchProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    m_PendingViewInfo.pitch = EditorGUILayout.Slider("pitch", m_PendingViewInfo.pitch, minPitchProp.floatValue, maxPitchProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(minPitchProp, -80, 0, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(maxPitchProp, 0, 80, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultYawProp, -180, 180, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    m_PendingViewInfo.yaw = EditorGUILayout.Slider("yaw", m_PendingViewInfo.yaw, -180, 180, GUILayout.ExpandWidth(true));

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Slider(defaultDistanceProp, minDistanceProp.floatValue, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();
                    m_PendingViewInfo.distance = EditorGUILayout.Slider("distance", m_PendingViewInfo.distance, minDistanceProp.floatValue, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(minDistanceProp, 0.2f, maxDistanceProp.floatValue, GUILayout.ExpandWidth(true));
                    EditorGUILayout.Slider(maxDistanceProp, minDistanceProp.floatValue, 80, GUILayout.ExpandWidth(true));

                    m_viObject.ApplyModifiedProperties();
                }
            }
            EditorGUI.EndDisabledGroup();
            
            

            ////////// 编辑相机震屏
            //GUILayout.Space(30);
            //EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            //{
            //    GUILayout.Label("编辑相机震屏", EGUIStyles.TitleTextStyle);

            //    ////// CameraEffectProfile
            //    effectProfile = (CameraEffectInfo)EditorGUILayout.ObjectField("CameraEffect Profile", effectProfile, typeof(CameraEffectInfo), true, GUILayout.ExpandWidth(true));
            //    EditorGUILayout.LabelField("资源路径：       " + cc.effectProfileAssetPath, GUILayout.ExpandWidth(true));
            //    if (effectProfile != null)
            //    {
            //        // 记录资源地址，方便执行“Save”
            //        string path = AssetDatabase.GetAssetPath(effectProfile);
            //        if (!string.IsNullOrEmpty(path))
            //        {
            //            cc.effectProfileAssetPath = path;
            //        }
            //    }

            //    ////// GO
            //    EditorGUILayout.Separator();
            //    if (GUILayout.Button("GO", GUILayout.Height(30)))
            //    {
            //        if (effectProfile != null)
            //        {
            //            CameraEffectInfo ep;
            //            cc.GetEffectProfile(out ep);

            //            if( ep == null || !CanSave(ep) )
            //            {
            //                ep = Object.Instantiate<CameraEffectInfo>(effectProfile);
            //            }
            //            effectProfile = ep;

            //            cc.PlayCameraEffect(effectProfile);
            //        }
            //    }

            //    ////// SAVE & SAVE AS
            //    EditorGUI.BeginDisabledGroup(!CanSave(effectProfile));
            //    {
            //        EditorGUILayout.BeginHorizontal();
            //        if (GUILayout.Button("Save", GUILayout.Width(200)))
            //        {
            //            AssetDatabase.CreateAsset(effectProfile, cc.effectProfileAssetPath);
            //            AssetDatabase.Refresh();
            //        }
            //        if (GUILayout.Button("Save as...", GUILayout.Width(200)))
            //        {
            //            string savePath = EditorUtility.SaveFilePanel("", cc.effectProfileAssetPath, "CameraEffectProfile", "asset");
            //            if (!string.IsNullOrEmpty(savePath))
            //            {
            //                savePath = savePath.Substring(Application.dataPath.Length - 6);
            //                AssetDatabase.CreateAsset(effectProfile, savePath);
            //                AssetDatabase.Refresh();
            //            }
            //        }
            //        EditorGUILayout.EndHorizontal();
            //    }
            //    EditorGUI.EndDisabledGroup();

            //    GUILayout.Space(10);
            //    GUILayout.Label("当前相机位参数", EGUIStyles.Label2);
            //    if( effectProfile != null )
            //    {
            //        Editor editor = CreateEditor(effectProfile);
            //        editor.OnInspectorGUI();
            //    }

            //}
            //EditorGUI.EndDisabledGroup();
        }

        // 判断资源是否可以保存，仅实例化资源可以被保存
        private bool CanSave(Object asset)
        {
            if (asset == null)
                return false;

            return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset));
        }

        private void EditorSetCharacterView(CameraViewInfoCollection.CharacterView charView, bool isAiming = false, float smoothTime = 0.15f)
        {
            cc.EditorSetCharacterView(charView, isAiming, smoothTime);

            RefreshViewInfoProperties();
        }
    }
}