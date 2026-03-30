using Microsoft.Xna.Framework;
using Raven.Graphics;

namespace Raven.Engine.Scene3D;

/// <summary>
/// Use for creating a tree of rooms which connect to each other through visibility planes
/// </summary>
//TODO add this cos the involved tech will be v handy
// try to use a 1x1 output shader to do a depth/occlusion test on the BSP planes?
// I would need to:
// draw the in-frustum BSP planes' depth to a blank RT2D
// create an instancing shader which can take a list of XYWH sections of the screen
// run said shader on the BSP depth RT2D and the current BSP's (precomputed?) depth
// (it could also be useful to write a generic shader for simple transforms/crops/color ops to reduce reliance on SpriteBatch)
// (also, write a replacement for MonoGame's cringe spritebatch which can handle things like megatextures by default, automatically)
// (also, implement megatextures)

public class ConnectedRoomBSPScene : Scene {
    public class RoomBSP {
        
    }

    public override void Save() {
        throw new System.NotImplementedException();
    }

    public override void Load() {
        throw new System.NotImplementedException();
    }

    public override void Spawn(Entity entity) {
        throw new System.NotImplementedException();
    }

    public override void Spawn(Entity entity, Vector3 position) {
        throw new System.NotImplementedException();
    }

    public override void Kill(Entity entity) {
        throw new System.NotImplementedException();
    }

    public override void Update() {
        throw new System.NotImplementedException();
    }

    public override void UpdatePhysics() {
        throw new System.NotImplementedException();
    }

    public override void PostPhysics() {
        throw new System.NotImplementedException();
    }

    public override void UpdateGraphics() {
        throw new System.NotImplementedException();
    }

    public override void Stabilize() {
        throw new System.NotImplementedException();
    }

    public override void Render(Camera camera, GBuffer gbuffer) {
        throw new System.NotImplementedException();
    }
}
