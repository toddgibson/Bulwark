using Godot;

namespace Bulwark.UI
{
    public class FPSLabel : Label
    {
        public void _on_Timer_timeout()
        {
            Text = $"FPS: {Godot.Engine.GetFramesPerSecond()}";
        }
    }
}
