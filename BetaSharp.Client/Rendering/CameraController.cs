using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Client.Options;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering;

public class CameraController
{
    private readonly BetaSharp _game;
    private readonly float _thirdPersonDistance = 4.0F;
    private readonly float _frontThirdPersonDistance = 4.0F;
    public float ViewBob { get; private set; }
    public float LastViewBob { get; private set; }

    private float _prevThirdPersonDistance = 4.0F;
    private float _prevFrontThirdPersonDistance = 4.0F;
    private readonly float _thirdPersonYaw = 0.0F;
    private float _prevThirdPersonYaw;
    private readonly float _thirdPersonPitch = 0.0F;
    private float _prevThirdPersonPitch;
    private readonly float _cameraRoll = 0.0F;
    private float _prevCameraRoll;
    private readonly float _cameraRollAmount = 0.0F;
    private float _prevCameraRollAmount;
    private bool _isZoomHeld;
    private float _zoomScale = 2.0F;
    public double CameraZoom { get; private set; } = 1.0D;
    public double CameraYaw { get; } = 0.0D;
    public double CameraPitch { get; } = 0.0D;
    public bool IsZoomActive => _isZoomHeld;

    public CameraController(BetaSharp game)
    {
        _game = game;
    }

    public void UpdateCamera()
    {
        LastViewBob = ViewBob;
        _prevThirdPersonDistance = _thirdPersonDistance;
        _prevFrontThirdPersonDistance = _frontThirdPersonDistance;
        _prevThirdPersonYaw = _thirdPersonYaw;
        _prevThirdPersonPitch = _thirdPersonPitch;
        _prevCameraRoll = _cameraRoll;
        _prevCameraRollAmount = _cameraRollAmount;

        float luminance = _game.World.GetLuminance(MathHelper.Floor(_game.Camera.X), MathHelper.Floor(_game.Camera.Y), MathHelper.Floor(_game.Camera.Z));
        float renderDistFactor = System.Math.Clamp((_game.Options.renderDistance - 4.0F) / 28.0F, 0.0F, 1.0F);
        float targetBob = luminance * (1.0F - renderDistFactor) + renderDistFactor;
        ViewBob += (targetBob - ViewBob) * 0.1F;
    }

    public void SetZoomState(bool isHeld, float zoomScale)
    {
        _isZoomHeld = isHeld;
        _zoomScale = System.Math.Clamp(zoomScale, 1.25F, 20.0F);
        CameraZoom = 1.0D;
    }

    public float GetFov(float tickDelta, bool isHand = false)
    {
        EntityLiving cameraEntity = _game.Camera;
        float fov = isHand ? 70.0F : (30.0F + _game.Options.Fov * 90.0F);

        if (cameraEntity.IsInFluid(Material.Water))
        {
            fov -= 10.0F;
        }

        if (cameraEntity.Health <= 0)
        {
            float deathTimeF = cameraEntity.DeathTime + tickDelta;
            fov /= (1.0F - 500.0F / (deathTimeF + 500.0F)) * 2.0F + 1.0F;
        }

        if (_isZoomHeld && !isHand)
        {
            float zoomProgress = 1.0F / _zoomScale;
            float easedZoomProgress = (float)System.Math.Pow(zoomProgress, 2.0D);
            fov = 1.0F + (fov - 1.0F) * easedZoomProgress;
        }

        return fov + _prevCameraRoll + (_cameraRoll - _prevCameraRoll) * tickDelta;
    }

    public void ApplyDamageTiltEffect(float tickDelta)
    {
        EntityLiving cameraEntity = _game.Camera;
        float hurtTimeF = cameraEntity.HurtTime - tickDelta;

        if (cameraEntity.Health <= 0)
        {
            float deathTimeF = cameraEntity.DeathTime + tickDelta;
            RenderDragon.Api.Rotate(40.0F - 8000.0F / (deathTimeF + 200.0F), 0.0F, 0.0F, 1.0F);
        }

        if (hurtTimeF >= 0.0F)
        {
            hurtTimeF /= cameraEntity.MaxHurtTime;
            hurtTimeF = MathHelper.Sin(hurtTimeF * hurtTimeF * hurtTimeF * hurtTimeF * (float)Math.PI);
            RenderDragon.ApplyDamageTilt(cameraEntity.AttackedAtYaw, hurtTimeF * 14.0F);
        }
    }

    public void ApplyViewBobbing(float tickDelta)
    {
        if (_game.Camera is EntityPlayer player)
        {
            float speedDelta = player.HorizontalSpeed - player.PrevHorizontalSpeed;
            float speed = -(player.HorizontalSpeed + speedDelta * tickDelta);
            float bobAmount = player.prevStepBobbingAmount + (player.stepBobbingAmount - player.prevStepBobbingAmount) * tickDelta;
            float pitch = player.CameraPitch + (player.Tilt - player.CameraPitch) * tickDelta;
            RenderDragon.ApplyViewBobbing(
                MathHelper.Sin(speed * (float)Math.PI) * bobAmount * 0.5F,
                -Math.Abs(MathHelper.Cos(speed * (float)Math.PI) * bobAmount),
                MathHelper.Sin(speed * (float)Math.PI) * bobAmount * 3.0F,
                Math.Abs(MathHelper.Cos(speed * (float)Math.PI - 0.2F) * bobAmount) * 5.0F + pitch);
        }
    }

    public void ApplyCameraTransform(float tickDelta)
    {
        EntityLiving cameraEntity = _game.Camera;
        float eyeHeightOffset = cameraEntity.StandingEyeHeight - 1.62F;
        float interpolatedPitch = cameraEntity.PrevPitch + (cameraEntity.Pitch - cameraEntity.PrevPitch) * tickDelta;
        float interpolatedYaw = cameraEntity.PrevYaw + (cameraEntity.Yaw - cameraEntity.PrevYaw) * tickDelta;
        double x = cameraEntity.PrevX + (cameraEntity.X - cameraEntity.PrevX) * (double)tickDelta;
        double y = cameraEntity.PrevY + (cameraEntity.Y - cameraEntity.PrevY) * (double)tickDelta - (double)eyeHeightOffset;
        double z = cameraEntity.PrevZ + (cameraEntity.Z - cameraEntity.PrevZ) * (double)tickDelta;

        RenderDragon.Api.Rotate(_prevCameraRollAmount + (_cameraRollAmount - _prevCameraRollAmount) * tickDelta, 0.0F, 0.0F, 1.0F);

        if (cameraEntity.isSleeping())
        {
            eyeHeightOffset = (float)((double)eyeHeightOffset + 1.0D);
            if (!_game.Options.DebugCamera)
            {
                float bedRotation = 0.0F;
                int blockId = _game.World.Reader.GetBlockId(MathHelper.Floor(cameraEntity.X), MathHelper.Floor(cameraEntity.Y), MathHelper.Floor(cameraEntity.Z));
                if (blockId == Block.Bed.id)
                {
                    int meta = _game.World.Reader.GetBlockMeta(MathHelper.Floor(cameraEntity.X), MathHelper.Floor(cameraEntity.Y), MathHelper.Floor(cameraEntity.Z));
                    bedRotation = (meta & 3) * 90.0F;
                }

                RenderDragon.ApplySleepingCameraTransform(0.3F, bedRotation, interpolatedYaw + 180.0F, interpolatedPitch);
            }
            else
            {
                RenderDragon.ApplyEyeHeightOffset(0.3F);
            }
        }
        else if (_game.Options.CameraMode == EnumCameraMode.ThirdPerson || _game.Options.CameraMode == EnumCameraMode.FrontThirdPerson)
        {
            double currentDistance;
            if (_game.Options.CameraMode == EnumCameraMode.FrontThirdPerson)
            {
                currentDistance = _prevFrontThirdPersonDistance + (_frontThirdPersonDistance - _prevFrontThirdPersonDistance) * tickDelta;
            }
            else
            {
                currentDistance = _prevThirdPersonDistance + (_thirdPersonDistance - _prevThirdPersonDistance) * tickDelta;
            }

            float targetPitch;
            float targetYaw;

            if (_game.Options.DebugCamera)
            {
                targetYaw = _prevThirdPersonYaw + (_thirdPersonYaw - _prevThirdPersonYaw) * tickDelta;
                targetPitch = _prevThirdPersonPitch + (_thirdPersonPitch - _prevThirdPersonPitch) * tickDelta;
                RenderDragon.ApplyThirdPersonDebugCameraTransform((float)currentDistance, targetPitch, targetYaw);
            }
            else
            {
                targetYaw = cameraEntity.Yaw;
                targetPitch = cameraEntity.Pitch;

                double vecX = (double)(-MathHelper.Sin(targetYaw / 180.0F * (float)Math.PI) * MathHelper.Cos(targetPitch / 180.0F * (float)Math.PI)) * currentDistance;
                double vecZ = (double)(MathHelper.Cos(targetYaw / 180.0F * (float)Math.PI) * MathHelper.Cos(targetPitch / 180.0F * (float)Math.PI)) * currentDistance;
                double vecY = (double)(-MathHelper.Sin(targetPitch / 180.0F * (float)Math.PI)) * currentDistance;

                for (int i = 0; i < 8; ++i)
                {
                    float offsetX = ((i & 1) * 2 - 1) * 0.1F;
                    float offsetY = ((i >> 1 & 1) * 2 - 1) * 0.1F;
                    float offsetZ = ((i >> 2 & 1) * 2 - 1) * 0.1F;

                    HitResult hit = new HitResult(HitResultType.MISS);

                    if (_game.Options.CameraMode == EnumCameraMode.FrontThirdPerson)
                    {
                        hit = _game.World.Reader.Raycast(
                            new Vec3D(x + offsetX, y + offsetY, z + offsetZ),
                            new Vec3D(x + vecX + offsetX + offsetZ, y + vecY + offsetY, z + vecZ + offsetZ)
                        );
                    }
                    else
                    {
                        hit = _game.World.Reader.Raycast(
                            new Vec3D(x + offsetX, y + offsetY, z + offsetZ),
                            new Vec3D(x - vecX + offsetX + offsetZ, y - vecY + offsetY, z - vecZ + offsetZ)
                        );
                    }

                    if (hit.Type != HitResultType.MISS)
                    {
                        double dist = hit.Pos.distanceTo(new Vec3D(x, y, z));
                        if (dist < currentDistance)
                        {
                            currentDistance = dist;
                        }
                    }
                }

                RenderDragon.ApplyThirdPersonChaseCameraTransform((float)currentDistance, _game.Options.CameraMode == EnumCameraMode.FrontThirdPerson);
            }
        }
        else
        {
            RenderDragon.ApplyNearPlaneOffset(0.1F);
        }

        if (!_game.Options.DebugCamera)
        {
            RenderDragon.ApplyCameraOrientation(interpolatedPitch, interpolatedYaw + 180.0F);
        }

        RenderDragon.ApplyEyeHeightOffset(eyeHeightOffset);
    }
}
