using Content.Shared.Eye;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Eye;

public sealed class DarkVisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DarkVisionComponent, ComponentInit>(OnVisionInit);
        SubscribeLocalEvent<DarkVisionComponent, ComponentShutdown>(OnVisionShutdown);

        SubscribeLocalEvent<DarkVisionComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DarkVisionComponent, PlayerDetachedEvent>(OnPlayerDetached);

        //SubscribeNetworkEvent<RequestUpdateOverlayMessage>(OnRequestOverlay);
    }

    /*
    private void OnRequestOverlay(RequestUpdateOverlayMessage message, EntitySessionEventArgs args)
    {
        if (_player.LocalPlayer?.UserId == message.Id)
        {

            if (TryComp<DarkVisionComponent>(_player.LocalPlayer?.ControlledEntity, out var vision))
            {
                vision.IsEnable = message.On;
                vision.DrawLight = vision.IsEnable && message.DrawLight;
                vision.LayerColor = message.LayerColor;
                vision.ShaderTexturePrototype = message.ShaderTexturePrototype;

                if (vision.IsEnable)
                {
                    if (vision.ShaderTexturePrototype != null)
                    {
                        _darkOverlay.SetShaderProto(vision.ShaderTexturePrototype);
                        _lightManager.DrawLighting = vision.DrawLight;
                        _overlayMan.AddOverlay(_darkOverlay);
                    }
                }
                else
                {
                    _overlayMan.RemoveOverlay(_darkOverlay);
                    _lightManager.DrawLighting = true;
                }
            }
        }
    }
    */

    private void OnPlayerAttached(EntityUid uid, DarkVisionComponent component, PlayerAttachedEvent args)
    {
        if (!component.IsEnable)
            return;

        if (component.ShaderTexturePrototype != null)
        {
            if (component.darkOverlay == null)
                component.darkOverlay = new DarkVisionOverlay();
            component.darkOverlay.SetShaderProto(component.ShaderTexturePrototype);
            _lightManager.DrawLighting = component.DrawLight;
            _overlayMan.AddOverlay(component.darkOverlay);
        }
    }

    private void OnPlayerDetached(EntityUid uid, DarkVisionComponent component, PlayerDetachedEvent args)
    {
        if (!component.IsEnable)
            return;

        if (component.darkOverlay == null)
        {
            component.darkOverlay = new DarkVisionOverlay();
        }
        else _overlayMan.RemoveOverlay(component.darkOverlay);

        _lightManager.DrawLighting = true;
    }

    private void OnVisionInit(EntityUid uid, DarkVisionComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _lightManager.DrawLighting = component.DrawLight;
            if (component.darkOverlay == null)
                component.darkOverlay = new DarkVisionOverlay();
            _overlayMan.AddOverlay(component.darkOverlay);
        }
    }

    private void OnVisionShutdown(EntityUid uid, DarkVisionComponent component, ComponentShutdown args)
    {
        if (!component.IsEnable)
            return;

        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _lightManager.DrawLighting = true;
            if (component.darkOverlay != null)
                _overlayMan.RemoveOverlay(component.darkOverlay);
        }
    }
}
