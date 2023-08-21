using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FontSettings.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework
{
    internal partial class SpriteFontGenerator
    {
        public static SpriteFont Generate(FontConfig config)
        {
            return SpriteFontGenerator.FromTtf(
                ttfPath: config.FontFilePath,
                fontIndex: config.FontIndex,
                fontPixelHeight: config.FontSize,
                characterRanges: config.CharacterRanges,
                spacing: config.Spacing,
                lineSpacing: (int)config.LineSpacing,
                charOffsetX: config.CharOffsetX,
                charOffsetY: config.CharOffsetY,
                defaultCharacter: config.TryGetInstance(out IWithDefaultCharacter withDefaultCharacter)
                    ? withDefaultCharacter.DefaultCharacter
                    : '*',
                mask: config.TryGetInstance(out IWithSolidColor withSolidColor)
                    ? withSolidColor.SolidColor
                    : Color.White);
        }

        public static Task<SpriteFont> GenerateAsync(FontConfig config)
        {
            return Task.Run(() => Generate(config));
        }

        public static async Task<SpriteFont> GenerateAsync(FontConfig config, CancellationToken token)
        {
            // 开始异步，让它先跑着。
            var task = GenerateAsync(config);

            // 在跑异步的期间，每隔100毫秒检查是否传入了取消请求，如果有，调ThrowIfCancellationRequested。
            while (!task.IsCompleted)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(100, token);
            }

            // 到这里异步已经跑完了，也过了可取消的界限，返回异步的值。
            return await task;
        }
    }
}
