using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RavenRPG.Engine;

public static class Resources {
    public static ContentManager content;
    public static string RootDirectory => content.RootDirectory;

    public static string FolderNameFromType(ContentType type) => type + "s";
    
    public enum ContentType {
        Texture, Font, Shader, Model//, Sound
    }

    public enum DataType {
        File, Procedural
    }
    
    public interface IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public ContentType Type { get; }
        public DataType DataType { get; }
        
        public double LastAccessTime { get; }
        
        public bool Loaded { get; set; }
        
        public void Load();
        
        public void Unload() {
            content.UnloadAsset(FullName);
            Loaded = false;
        }
    }
    
    public class ContentDataTexture : IContentData {
        public string Name { get; }
        public string FullName { get; }
        public double LastAccessTime { get; }
        public bool Loaded { get; set; } = false;
        double _last_access_time = -1;
        
        public ContentType Type { get; } = ContentType.Texture;
        public DataType DataType { get; }

        Texture2D _texture; public Texture2D Texture {
            get {
                if (!Loaded) Load();
                return _texture;
            }
        }

        public ContentDataTexture(string full_name) {
            FullName = full_name;
            Name = full_name.Remove(0, full_name.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
            DataType = DataType.File;
        }

        public ContentDataTexture(string name, Texture2D texture) {
            FullName = FolderNameFromType(ContentType.Texture) + "/" + name;
            Name = name;
            _texture = texture;
            DataType = DataType.Procedural;
            Loaded = true;
            
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _texture = content.Load<Texture2D>(FullName);
            Loaded = true;
        }
    }
    
    public class ContentDataShader : IContentData {
        public string Name { get; }
        public string FullName { get; }
        public double LastAccessTime { get; }
        public bool Loaded { get; set; } = false;
        double _last_access_time = -1;

        public ContentType Type { get; } = ContentType.Shader;
        public DataType DataType { get; } = DataType.File;

        Effect _shader; public Effect Shader {
            get {
                if (!Loaded) Load();
                return _shader;
            }
        }

        public ContentDataShader(string full_name) {
            FullName = full_name;
            Name = full_name.Remove(0, full_name.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _shader = content.Load<Effect>(FullName);
            Loaded = true;
        }
    }
    
    public class ContentDataFont : IContentData {
        public string Name { get; }
        public string FullName { get; }
        public double LastAccessTime { get; }
        public bool Loaded { get; set; } = false;
        double _last_access_time = -1;

        public ContentType Type { get; } = ContentType.Font;
        public DataType DataType { get; } = DataType.File;

        SpriteFont _font; public SpriteFont Font {
            get {
                if (!Loaded) Load();
                return _font;
            }
        }

        public ContentDataFont(string full_name) {
            FullName = full_name;
            Name = full_name.Remove(0, full_name.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _font = content.Load<SpriteFont>(FullName);
            Loaded = true;
        }

    }
    
    public class ContentDataModel : IContentData {
        public string Name { get; }
        public string FullName { get; }
        public double LastAccessTime { get; }
        public bool Loaded { get; set; } = false;
        double _last_access_time = -1;

        public ContentType Type { get; } = ContentType.Shader;
        public DataType DataType { get; } = DataType.File;

        Model _model; public Model Model {
            get {
                if (!Loaded) Load();
                return _model;
            }
        }

        public ContentDataModel(string full_name) {
            FullName = full_name;
            Name = full_name.Remove(0, full_name.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _model = content.Load<Model>(FullName);
            Loaded = true;
        }
    }

    static Dictionary<string, IContentData> all_content = new ();

    public static Texture2D GetTexture(string name) {
        return ((ContentDataTexture)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]).Texture;
    }
    public static ContentDataTexture GetTextureContent(string name) {
        return ((ContentDataTexture)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]);
    }
    public static Effect GetShader(string name) {
        return ((ContentDataShader)all_content[$"{FolderNameFromType(ContentType.Shader)}/{name}"]).Shader;
    }
    public static SpriteFont GetFont(string name) {
        return ((ContentDataFont)all_content[$"{FolderNameFromType(ContentType.Font)}/{name}"]).Font;
    }
    public static Model GetModel(string name) {
        return ((ContentDataModel)all_content[$"{FolderNameFromType(ContentType.Model)}/{name}"]).Model;
    }

    public static void AddTexture(string name, Texture2D texture) {
        all_content.Add($"Textures/{name}", new ContentDataTexture(name, texture));
    }
    
    static void add_content_of_type(ContentType type, int path_crop_length) {
        if (Directory.Exists(content.RootDirectory + "/" + FolderNameFromType(type))) {
            foreach (var file in Directory.GetFiles(content.RootDirectory + "/" + FolderNameFromType(type), "*.xnb", SearchOption.AllDirectories)) {
                var fi = new FileInfo(file);
                var rp = fi.FullName.Remove(0, path_crop_length);
                rp = rp.Remove(rp.Length - 4);

                switch (type) {
                    case ContentType.Texture:
                        all_content.Add(rp, new ContentDataTexture(rp));
                        break;
                    case ContentType.Font:
                        all_content.Add(rp, new ContentDataFont(rp));
                        break;
                    case ContentType.Shader:
                        all_content.Add(rp, new ContentDataShader(rp));
                        break;
                    case ContentType.Model:
                        all_content.Add(rp, new ContentDataModel(rp));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }
    
    public static void LoadContentList(ContentManager content) {
        Resources.content = content;

        var path_crop_length = Environment.CurrentDirectory.Length + 1;
        path_crop_length += "Content/".Length;
        
        add_content_of_type(ContentType.Texture, path_crop_length);
        add_content_of_type(ContentType.Shader, path_crop_length);
        add_content_of_type(ContentType.Model, path_crop_length);
        add_content_of_type(ContentType.Font, path_crop_length);
    }

    public static string ListAllContent() {
        string output = "";
        foreach (IContentData content in all_content.Values.OrderBy(a => a.FullName)) {
            output += $"{content.FullName} :: {(content.DataType == DataType.Procedural ? "[RAM]" : "[DSK]")} {(content.Loaded ? "[LOADED]" : "")}\n";
        }
        return output;
    }
    public static string ListAllLoadedContent() {
        string output = "";
        foreach (IContentData content in all_content.Values.Where(a => a.Loaded).OrderBy(a => a.FullName)) {
            output += $"{content.FullName} {(content.Loaded ? "[LOADED]" : "")}\n";
        }
        return output;
    }
}