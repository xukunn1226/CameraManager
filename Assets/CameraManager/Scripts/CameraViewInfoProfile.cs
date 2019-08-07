using UnityEngine;

namespace Framework
{
    [CreateAssetMenu(menuName = "创建相机位集合数据", fileName = "CameraViewInfoProfile")]
    public class CameraViewInfoProfile : ScriptableObject
    {
        public CameraViewInfo   freeView;                   // 3D自由视角
        public CameraViewInfo   closeupView;                // 特写视角：半身像
        public CameraViewInfo   extremeCloseupView;         // 特写视角：脸部
        public CameraViewInfo   mailView;                   // 灵符视角
        public CameraViewInfo   openGSUIView;               // 指引精灵视角
        public CameraViewInfo   dialogPositiveView;         // NPC对话视角（正面）
        public CameraViewInfo   dialogSideView;             // NPC对话视角（旁边）

        static public CameraViewInfoProfile CopyFrom(CameraViewInfoProfile src)
        {
            CameraViewInfoProfile dst           = Instantiate(src);
            dst.freeView                        = Instantiate(src.freeView);
            dst.closeupView                     = Instantiate(src.closeupView);
            dst.extremeCloseupView              = Instantiate(src.extremeCloseupView);
            dst.mailView                        = Instantiate(src.mailView);
            dst.openGSUIView                    = Instantiate(src.openGSUIView);
            dst.dialogPositiveView              = Instantiate(src.dialogPositiveView);
            dst.dialogSideView                  = Instantiate(src.dialogSideView);

            // 初始化2.5D视角下的pitch
            //if (KGameApp.instance)
            //{
            //    dst.freeView.fixedPitch = DataCenter.instance.ConstConfig.FixedPitch;
            //}
            //else
            {
                dst.freeView.fixedPitch = 40f;
            }
            
            dst.closeupView.fixedPitch          = dst.closeupView.defaultPitch;
            dst.extremeCloseupView.fixedPitch   = dst.extremeCloseupView.defaultPitch;
            dst.mailView.fixedPitch             = dst.mailView.defaultPitch;
            dst.openGSUIView.fixedPitch         = dst.openGSUIView.defaultPitch;
            dst.dialogPositiveView.fixedPitch   = dst.dialogPositiveView.defaultPitch;
            dst.dialogSideView.fixedPitch       = dst.dialogSideView.defaultPitch;
            return dst;
        }
    }
}