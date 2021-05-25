using Godot;

namespace Bulwark
{
    public class CameraRig : Spatial
    {
        public Transform? FollowTransform;

        private Camera Camera;

        [Export] private float NormalSpeed = 0.25f;
        [Export] private float FastSpeed = 0.5f;
        [Export] private float MovementSpeed = 0.25f;
        [Export] private float DragMovementSpeed = 0.05f;
        [Export] private float MovementTime = 3f;
        [Export] private float RotationAmount = 0.015f;
        [Export] private float DragRotationAmount = 25f;
        [Export] private Vector3 ZoomAmount = new Vector3(0f, 1f, 1f);
        [Export] private float MinZoom = 4f;
        [Export] private float MaxZoom = 15f;
        [Export] private Vector3 MinPosition = new Vector3(-40f, 0f, -40f);
        [Export] private Vector3 MaxPosition = new Vector3(40f, 0f, 40f);

        [Export] private Vector3 StartPosition = Vector3.Zero;
        [Export] private Vector3 StartZoom = new Vector3(0f, 15f, 15f);

        [Export] private bool EnableAutoRotate;
        [Export] private float AutoRotateSpeed = 0.005f;

        private Vector3 NewPosition;
        private Quat NewRotation;
        private Vector3 NewZoom;

        private bool IsDragMoving;
        private bool IsDragRotating;

        public override void _Ready()
        {
            Camera = GetNode<Camera>("Camera");
            
            Translation = StartPosition;
            Camera.Translation = StartZoom;

            NewPosition = Transform.origin;
            NewRotation = Transform.basis.Quat();
            NewZoom = Camera.Translation;
        }

        public override void _Process(float delta)
        {
            if (FollowTransform != null)
            {
                GlobalTransform = (Transform)FollowTransform;
            }
            else if (EnableAutoRotate)
            {
                RotateY(AutoRotateSpeed * delta);
            }
            else
            {
                HandleMovementInput(delta);
            }
            
            UpdateEnvironment();

            if (Input.IsActionJustPressed("ui_cancel"))
                FollowTransform = null;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (EnableAutoRotate) return;

            if (@event is InputEventMouseButton)
            {
                var emb = (InputEventMouseButton)@event;

                // handle mouse wheel zoom
                if (emb.ButtonIndex == (int)ButtonList.WheelUp)
                {
                    NewZoom += emb.Factor * -ZoomAmount;
                }
                else if (emb.ButtonIndex == (int)ButtonList.WheelDown)
                {
                    NewZoom += emb.Factor * ZoomAmount;
                }
            }

            if (@event is InputEventMouseMotion)
            {
                var drag = (InputEventMouseMotion)@event;

                if (IsDragMoving)
                {
                    NewPosition += -Transform.basis.z * drag.Relative.y * DragMovementSpeed
                                + -Transform.basis.x * drag.Relative.x * DragMovementSpeed;
                }

                if (IsDragRotating)
                {
                    NewRotation *= new Quat(Vector3.Up * (Mathf.Clamp(-drag.Relative.x, -2f, 2f) / DragRotationAmount));
                }
            }
        }

        private void HandleMovementInput(float delta)
        {
            IsDragMoving = Input.IsActionPressed("map_drag_mouse");
            IsDragRotating = Input.IsActionPressed("map_rotate_mouse");

            MovementSpeed = Input.IsActionPressed("map_fast") ? FastSpeed : NormalSpeed;

            if (Input.IsActionPressed("map_up"))
            {
                NewPosition += -Transform.basis.z * MovementSpeed;
            }

            if (Input.IsActionPressed("map_down"))
            {
                NewPosition += -Transform.basis.z * -MovementSpeed;
            }

            if (Input.IsActionPressed("map_left"))
            {
                NewPosition += Transform.basis.x * -MovementSpeed;
            }

            if (Input.IsActionPressed("map_right"))
            {
                NewPosition += Transform.basis.x * MovementSpeed;
            }

            if (Input.IsActionPressed("map_rotate_left"))
            {
                NewRotation *= new Quat(Vector3.Up * RotationAmount);
            }

            if (Input.IsActionPressed("map_rotate_right"))
            {
                NewRotation *= new Quat(Vector3.Up * -RotationAmount);
            }

            if (Input.IsActionPressed("map_zoom_in"))
            {
                NewZoom += ZoomAmount;
            }

            if (Input.IsActionPressed("map_zoom_out"))
            {
                NewZoom -= ZoomAmount;
            }

            NewPosition = new Vector3(Mathf.Clamp(NewPosition.x, MinPosition.x, MaxPosition.x), NewPosition.y, Mathf.Clamp(NewPosition.z, MinPosition.z, MaxPosition.z));
            NewZoom = new Vector3(NewZoom.x, Mathf.Clamp(NewZoom.y, MinZoom, MaxZoom), Mathf.Clamp(NewZoom.z, MinZoom, MaxZoom));

            Translation = Translation.LinearInterpolate(NewPosition, delta * MovementTime);
            Rotation = new Quat(Rotation).Slerp(NewRotation.Normalized(), delta * MovementTime).GetEuler();
            Camera.Translation = Camera.Translation.LinearInterpolate(NewZoom, delta * MovementTime);
        }

        private void UpdateEnvironment()
        {
            if (Camera.Environment.DofBlurFarEnabled)
                Camera.Environment.DofBlurFarDistance = NewZoom.z * 2f;
        }
    }
}