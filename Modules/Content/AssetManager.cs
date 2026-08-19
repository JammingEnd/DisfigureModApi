using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DisfigureModApi.Modules.Content
{
    public static class AssetManager
    {
        private static readonly Dictionary<string, Texture2D> _textures = new();
        private static readonly Dictionary<string, Sprite> _sprites = new();

        public static Texture2D LoadTexture(string path)
        {
            path = Path.GetFullPath(path);

            if (_textures.TryGetValue(path, out Texture2D texture))
                return texture;

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Texture not found: {path}"
                );

            byte[] data = File.ReadAllBytes(path);

            texture = new Texture2D(2, 2);

            if (!ImageConversion.LoadImage(texture, data))
            {
                UnityEngine.Object.Destroy(texture);
                throw new Exception(
                    $"Failed to load texture: {path}"
                );
            }

            _textures[path] = texture;

            return texture;
        }

        public static Sprite LoadSprite(
            string path,
            float pixelsPerUnit = 100f)
        {
            path = Path.GetFullPath(path);

            if (_sprites.TryGetValue(path, out Sprite sprite))
                return sprite;

            Texture2D texture = LoadTexture(path);

            sprite = Sprite.Create(
                texture,
                new Rect(
                    0,
                    0,
                    texture.width,
                    texture.height
                ),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit
            );

            _sprites[path] = sprite;

            return sprite;
        }

        public static void UnloadSprite(string path)
        {
            path = Path.GetFullPath(path);

            if (_sprites.Remove(path, out Sprite sprite))
            {
                UnityEngine.Object.Destroy(sprite);
            }

            if (_textures.Remove(path, out Texture2D texture))
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        public static void UnloadAll()
        {
            foreach (Sprite sprite in _sprites.Values)
                UnityEngine.Object.Destroy(sprite);

            foreach (Texture2D texture in _textures.Values)
                UnityEngine.Object.Destroy(texture);

            _sprites.Clear();
            _textures.Clear();
        }
    }
}