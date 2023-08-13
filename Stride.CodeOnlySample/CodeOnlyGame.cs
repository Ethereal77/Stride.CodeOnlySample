// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System.Reflection;

using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;

namespace Stride.CodeOnlySample
{
    public class CodeOnlyGame : Game
    {
        private readonly Matrix view = Matrix.LookAtRH(eye: new Vector3(0, 0, 5),
                                                       target: new Vector3(0, 0, 0),
                                                       up: Vector3.UnitY);
        private EffectInstance simpleEffect;
        private GeometricPrimitive teapot;

        protected async override Task LoadContent()
        {
            await base.LoadContent();

            // Prepare effect/shader
            simpleEffect = new EffectInstance(new Effect(GraphicsDevice, SpriteEffect.Bytecode));

            // Load the texture and set it to the shader / effect
            var texturePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "small_uv.png");
            using var stream = new FileStream(texturePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            simpleEffect.Parameters.Set(TexturingKeys.Texture0, Texture.Load(GraphicsDevice, stream));

            // Create a teapot mesh
            teapot = GeometricPrimitive.Teapot.New(GraphicsDevice);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Clear the backbuffer
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.CornflowerBlue);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer | DepthStencilClearOptions.Stencil);

            // Set the Z-Buffer and render target
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            var deltaTime = (float) gameTime.Total.TotalSeconds;

            // Compute matrices
            var world = Matrix.Scaling(MathF.Sin(deltaTime * 1.5f) * 0.2f + 1) *
                        Matrix.RotationX(deltaTime) * Matrix.RotationY(deltaTime * 2) * Matrix.RotationZ(deltaTime * 0.7f) *
                        Matrix.Translation(0, 0, 0);

            var aspectRatio = (float) GraphicsDevice.Presenter.BackBuffer.ViewWidth / GraphicsDevice.Presenter.BackBuffer.ViewHeight;

            var projection = Matrix.PerspectiveFovRH(fov: MathF.PI / 4, aspectRatio, znear: 0.1f, zfar: 100.0f);

            // Setup the effect / shader
            simpleEffect.Parameters.Set(SpriteBaseKeys.MatrixTransform, Matrix.Multiply(world, Matrix.Multiply(view, projection)));
            simpleEffect.UpdateEffect(GraphicsDevice);

            // Draw
            teapot.Draw(GraphicsContext, simpleEffect);
        }
    }
}
