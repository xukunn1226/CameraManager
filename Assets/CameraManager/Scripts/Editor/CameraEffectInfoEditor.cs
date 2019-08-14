//using UnityEngine;
//using System;
//using UnityEditor;
//using System.Diagnostics;

//namespace Framework
//{
//    [CustomEditor(typeof(CameraEffectInfo))]
//    public class CameraEffectInfoEditor : Editor
//    {
//        //        private SerializedProperty durationProp;

//        //        private SerializedProperty priorityProp;

//        //        private SerializedProperty shakePositionProp;
//        //        private SerializedProperty shakePositionActiveProp;
//        //        private SerializedProperty shakePositionStrengthProp;
//        //        private SerializedProperty shakePositionXCurveProp;
//        //        private SerializedProperty shakePositionYCurveProp;
//        //        private SerializedProperty shakePositionZCurveProp;

//        //        private SerializedProperty shakeRotationProp;
//        //        private SerializedProperty shakeRotationActiveProp;
//        //        private SerializedProperty shakeRotationStrengthProp;
//        //        private SerializedProperty shakeRotationXCurveProp;
//        //        private SerializedProperty shakeRotationYCurveProp;
//        //        private SerializedProperty shakeRotationZCurveProp;

//        //        private SerializedProperty shakeFOVProp;
//        //        private SerializedProperty shakeFOVActiveProp;
//        //        private SerializedProperty shakeFOVMinScaleProp;
//        //        private SerializedProperty shakeFOVCurveProp;

//        //        private void OnEnable()
//        //        {
//        //            durationProp = serializedObject.FindProperty("duration");

//        //            priorityProp = serializedObject.FindProperty("priority");

//        //            shakePositionProp = serializedObject.FindProperty("shakePosition");
//        //            shakePositionActiveProp = shakePositionProp.FindPropertyRelative("active");
//        //            shakePositionStrengthProp = shakePositionProp.FindPropertyRelative("strength");
//        //            shakePositionXCurveProp = shakePositionProp.FindPropertyRelative("xCurve");
//        //            shakePositionYCurveProp = shakePositionProp.FindPropertyRelative("yCurve");
//        //            shakePositionZCurveProp = shakePositionProp.FindPropertyRelative("zCurve");

//        //            shakeRotationProp = serializedObject.FindProperty("shakeRotation");
//        //            shakeRotationActiveProp = shakeRotationProp.FindPropertyRelative("active");
//        //            shakeRotationStrengthProp = shakeRotationProp.FindPropertyRelative("strength");
//        //            shakeRotationXCurveProp = shakeRotationProp.FindPropertyRelative("xCurve");
//        //            shakeRotationYCurveProp = shakeRotationProp.FindPropertyRelative("yCurve");
//        //            shakeRotationZCurveProp = shakeRotationProp.FindPropertyRelative("zCurve");

//        //            shakeFOVProp = serializedObject.FindProperty("shakeFOV");
//        //            shakeFOVActiveProp = shakeFOVProp.FindPropertyRelative("active");
//        //            shakeFOVMinScaleProp = shakeFOVProp.FindPropertyRelative("minScaleOfFOV");
//        //            shakeFOVCurveProp = shakeFOVProp.FindPropertyRelative("dampCurve");
//        //        }

//        //        public override void OnInspectorGUI()
//        //        {
//        //            serializedObject.Update();

//        //            // duration
//        //            EditorGUILayout.DelayedFloatField(durationProp, GUILayout.ExpandWidth(true));

//        //            EditorGUILayout.DelayedIntField(priorityProp, GUILayout.ExpandWidth(true));

//        //            // shake position
//        //            GUILayout.Space(10);
//        //            GUI.color = Color.green;
//        //            shakePositionActiveProp.boolValue = EditorGUILayout.Toggle("激活Position震屏", shakePositionActiveProp.boolValue, GUILayout.ExpandWidth(true));
//        //            if (shakePositionActiveProp.boolValue)
//        //            {
//        //                GUI.color = Color.white;
//        //                shakePositionStrengthProp.vector3Value = EditorGUILayout.Vector3Field("震屏强度", shakePositionStrengthProp.vector3Value, GUILayout.ExpandWidth(true));
//        //                shakePositionXCurveProp.animationCurveValue = EditorGUILayout.CurveField("X轴震屏曲线", shakePositionXCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //                shakePositionYCurveProp.animationCurveValue = EditorGUILayout.CurveField("Y轴震屏曲线", shakePositionYCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //                shakePositionZCurveProp.animationCurveValue = EditorGUILayout.CurveField("Z轴震屏曲线", shakePositionZCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //            }

//        //            // shake rotation
//        //            GUILayout.Space(10);
//        //            GUI.color = Color.green;
//        //            shakeRotationActiveProp.boolValue = EditorGUILayout.Toggle("激活Rotation震屏", shakeRotationActiveProp.boolValue, GUILayout.ExpandWidth(true));
//        //            if (shakeRotationActiveProp.boolValue)
//        //            {
//        //                GUI.color = Color.white;
//        //                shakeRotationStrengthProp.vector3Value = EditorGUILayout.Vector3Field("震屏强度", shakeRotationStrengthProp.vector3Value, GUILayout.ExpandWidth(true));
//        //                shakeRotationXCurveProp.animationCurveValue = EditorGUILayout.CurveField("X轴震屏曲线", shakeRotationXCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //                shakeRotationYCurveProp.animationCurveValue = EditorGUILayout.CurveField("Y轴震屏曲线", shakeRotationYCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //                shakeRotationZCurveProp.animationCurveValue = EditorGUILayout.CurveField("Z轴震屏曲线", shakeRotationZCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //            }

//        //            // shake fov
//        //            GUILayout.Space(10);
//        //            GUI.color = Color.green;
//        //            shakeFOVActiveProp.boolValue = EditorGUILayout.Toggle("激活fov震屏", shakeFOVActiveProp.boolValue, GUILayout.ExpandWidth(true));
//        //            if (shakeFOVActiveProp.boolValue)
//        //            {
//        //                GUI.color = Color.white;
//        //                shakeFOVMinScaleProp.floatValue = EditorGUILayout.FloatField("最小FOV缩放系数", shakeFOVMinScaleProp.floatValue, GUILayout.ExpandWidth(true));
//        //                shakeFOVCurveProp.animationCurveValue = EditorGUILayout.CurveField("震屏曲线", shakeFOVCurveProp.animationCurveValue, GUILayout.ExpandWidth(true));
//        //            }

//        //            serializedObject.ApplyModifiedProperties();
//        //        }

//        public override void OnInspectorGUI()
//        {
//            base.DrawDefaultInspector();
//            GUI.color = Color.cyan;
//            if (GUILayout.Button("保存",GUILayout.Width(150),GUILayout.Height(50)))
//            {
//                SaveFunc();
//            }
//            GUI.color = Color.white;
//        }

//        void SaveFunc()
//        {
//            EditorUtility.SetDirty(this.target);
//            AssetDatabase.SaveAssets();
//            //UnityEngine.Debug.Log("Assets/Scripts/KGame/GamePlay/Camera/Editor/CameraEffectProfileEditor.cs");
//        }


//        Stopwatch stopwatch;

//        private void OnEnable()
//        {
//            stopwatch = new Stopwatch();
//            stopwatch.Start();
//            if (UpdateFunc == null)
//            {
//                UpdateFunc = Func;
//            }
//            //UnityEngine.Debug.LogFormat("<color=red>OnEnable : {0}</color>", AssetDatabase.GetAssetPath(this.target));
//            EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, UpdateFunc);
//        }

//        private void OnDisable()
//        {
//            //UnityEngine.Debug.LogFormat("<color=red>OnDisable : {0}</color>", AssetDatabase.GetAssetPath(this.target));

//            stopwatch.Stop();
//            EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, UpdateFunc);
//        }

//        long saveTime = 30000;

//        static EditorApplication.CallbackFunction UpdateFunc;

//        void Func()
//        {
//            if (stopwatch.ElapsedMilliseconds >= saveTime)
//            {
//                SaveFunc();
//                stopwatch.Reset();
//                stopwatch.Start();
//            }
//        }
//    }
//}