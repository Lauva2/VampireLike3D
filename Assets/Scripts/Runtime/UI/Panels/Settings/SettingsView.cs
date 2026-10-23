using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SettingsView : UIView
    {
        public Slider musicSlider;
        public Slider soundSlider;
        public void Present(SettingsModel model)
        {
            musicSlider.SetValueWithoutNotify(model.music); soundSlider.SetValueWithoutNotify(model.sound);
            SetText("QualityLabel", "画面质量 · " + new[] { "流畅", "均衡", "精致" }[model.quality]);
            SetText("FullscreenLabel", model.fullscreen ? "显示模式 · 全屏" : "显示模式 · 窗口");
        }
    }
}
