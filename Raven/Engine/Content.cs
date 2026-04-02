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
using Raven.Graphics;

namespace Raven.Engine;

public static class Resources {
    public static ContentManager engine_content;
    
    public static string FolderNameFromType(ContentType type) => type + "s";
    
    public enum ContentType {
        Texture, Font, Shader, Model, RenderTarget, GBuffer//, Sound
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
        
        public ContentManager content_manager_parent { get; set; } 
        
        public void Load();
        
        public void Unload() {
            content_manager_parent.UnloadAsset(FullName);
            Loaded = false;
        }

        public static string NormalizePath(string path) {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return path.Replace('\\', '/');
            else return path;
        }
    }
    
    public class ContentDataTexture : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = false;
        
        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.Texture;
        public DataType DataType { get; }

        Texture2D _texture;

        public Texture2D Texture {
            get {
                if (!Loaded) Load();
                return _texture;
            }
        }

        public ContentDataTexture(string full_name, ContentManager content) {
            content_manager_parent = content;
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/') + 1);
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
            _texture = content_manager_parent.Load<Texture2D>(FullName);
            Loaded = true;
        }
    }
    
    public class ContentDataShader : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = false;
        
        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.Shader;
        public DataType DataType { get; } = DataType.File;

        Effect _shader; 
        public Effect Shader {
            get {
                if (!Loaded) Load();
                return _shader;
            }
        }
        
        public Effect ShaderInstance => content_manager_parent.Load<Effect>(FullName); 

        public ContentDataShader(string full_name, ContentManager content) {
            content_manager_parent = content;
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _shader = content_manager_parent.Load<Effect>(FullName);
            Loaded = true;
        }
    }
    
    public class ContentDataFont : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = false;

        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.Font;
        public DataType DataType { get; } = DataType.File;

        SpriteFont _font; public SpriteFont Font {
            get {
                if (!Loaded) Load();
                return _font;
            }
        }

        public ContentDataFont(string full_name, ContentManager content) {
            content_manager_parent = content;
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _font = content_manager_parent.Load<SpriteFont>(FullName);
            Loaded = true;
        }

    }
    
    public class ContentDataModel : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = false;

        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.Shader;
        public DataType DataType { get; } = DataType.File;

        Model _model; public Model Model {
            get {
                if (!Loaded) Load();
                return _model;
            }
        }

        public ContentDataModel(string full_name, ContentManager content) {
            content_manager_parent = content;
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/')+1);
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }

        public void Load() {
            _model = content_manager_parent.Load<Model>(FullName);
            Loaded = true;
        }
    }

    public class ContentDataRenderTarget : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = true;

        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.RenderTarget;
        public DataType DataType { get; } = DataType.Procedural;

        RenderTarget2D _target;
        public RenderTarget2D RenderTarget => _target;

        public Action DrawToTargetAction { get; set; }
        
        //TODO add double buffering
        
        public ContentDataRenderTarget(string full_name, RenderTarget2D target) {
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/')+1);
            _target = target;
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }
        
        public void Load() {}
    }

    public class ContentDataGBuffer : IContentData {
        public string Name { get; }
        public string FullName { get; }
        
        public double LastAccessTime { get; }
        double _last_access_time = -1;
        
        public bool Loaded { get; set; } = true;

        public ContentManager content_manager_parent { get; set; }
        
        public ContentType Type { get; } = ContentType.GBuffer;
        public DataType DataType { get; } = DataType.Procedural;

        GBuffer _target;
        public GBuffer GBuffer => _target;

        public Action DrawAction { get; set; }
        
        public ContentDataGBuffer(string full_name, GBuffer target) {
            FullName = IContentData.NormalizePath(full_name);
            Name = FolderNameFromType(Type) + "/" + FullName.Remove(0, FullName.IndexOf('/')+1);
            _target = target;
            Debug.WriteLine($"Content added: {Name} :: {FullName} :: {Type.ToString()}");
        }
        
        public void Load() {}
    }
    
    static Dictionary<string, IContentData> all_content = new ();

    public static Texture2D GetTexture(string name) {
        return ((ContentDataTexture)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]).Texture;
    }
    public static ContentDataTexture GetTextureContent(string name) {
        return ((ContentDataTexture)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]);
    }
    
    public static RenderTarget2D GetRenderTarget2D(string name) {
        return ((ContentDataRenderTarget)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]).RenderTarget;
    }
    public static ContentDataRenderTarget GetRenderTarget2DContent(string name) {
        return ((ContentDataRenderTarget)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]);
    }
    
    public static GBuffer GetGBuffer(string name) {
        return ((ContentDataGBuffer)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]).GBuffer;
    }
    public static ContentDataGBuffer GetGBufferContent(string name) {
        return ((ContentDataGBuffer)all_content[$"{FolderNameFromType(ContentType.Texture)}/{name}"]);
    }
    
    public static Effect GetShader(string name) {
        return ((ContentDataShader)all_content[$"{FolderNameFromType(ContentType.Shader)}/{name}"]).Shader;
    }
    public static Effect GetShaderInstance(string name) {
        return ((ContentDataShader)all_content[$"{FolderNameFromType(ContentType.Shader)}/{name}"]).ShaderInstance;
    }
    public static ContentDataShader GetShaderContent(string name) {
        return ((ContentDataShader)all_content[$"{FolderNameFromType(ContentType.Shader)}/{name}"]);
    }
    
    public static SpriteFont GetFont(string name) {
        return ((ContentDataFont)all_content[$"{FolderNameFromType(ContentType.Font)}/{name}"]).Font;
    }
    public static Model GetModel(string name) {
        return ((ContentDataModel)all_content[$"{FolderNameFromType(ContentType.Model)}/{name}"]).Model;
    }

    public static void AddTexture(string name, Texture2D texture) {
        all_content.Add($"{FolderNameFromType(ContentType.Texture)}/{name}", new ContentDataTexture(name, texture));
    }
    public static void AddRenderTarget(string name, RenderTarget2D target) {
        all_content.Add($"{FolderNameFromType(ContentType.RenderTarget)}/{name}", new ContentDataRenderTarget(name, target));
    }
    public static void AddGBuffer(string name, GBuffer buffer) {
        all_content.Add($"{FolderNameFromType(ContentType.GBuffer)}/{name}", new ContentDataGBuffer(name, buffer));
    }
    
    static void add_content_of_type(ContentManager content, ContentType type, int path_crop_length) {
        if (Directory.Exists(content.RootDirectory + "/" + FolderNameFromType(type))) {
            foreach (var file in Directory.GetFiles(content.RootDirectory + "/" + FolderNameFromType(type), "*.xnb", SearchOption.AllDirectories)) {
                var fi = new FileInfo(file);
                var rp = fi.FullName.Remove(0, path_crop_length);
                rp = IContentData.NormalizePath(rp.Remove(rp.Length - 4));

                if (rp.StartsWith('/')) rp = rp.Remove(0, 1);
                
                switch (type) {
                    case ContentType.Texture:
                        all_content.Add(rp, new ContentDataTexture(rp, content));
                        break;
                    case ContentType.Font:
                        all_content.Add(rp, new ContentDataFont(rp, content));
                        break;
                    case ContentType.Shader:
                        all_content.Add(rp, new ContentDataShader(rp, content));
                        break;
                    case ContentType.Model:
                        all_content.Add(rp, new ContentDataModel(rp, content));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }

    static void add_all_engine_content(int path_crop_length) {
        
    }

    internal static void LoadEngineContent(ContentManager content) {
        engine_content = new ContentManager(content.ServiceProvider, content.RootDirectory + "/Engine");
        
        var path_crop_length = Environment.CurrentDirectory.Length + 1;
        path_crop_length += engine_content.RootDirectory.Length;
        
        add_content_of_type(engine_content, ContentType.Texture, path_crop_length);
        add_content_of_type(engine_content, ContentType.Shader, path_crop_length);
        add_content_of_type(engine_content, ContentType.Model, path_crop_length);
        add_content_of_type(engine_content, ContentType.Font, path_crop_length);
    }
    
    public static void LoadContentList(ContentManager content) {
        var path_crop_length = Environment.CurrentDirectory.Length + 1;
        path_crop_length += content.RootDirectory.Length;
        
        add_content_of_type(content, ContentType.Texture, path_crop_length);
        add_content_of_type(content, ContentType.Shader, path_crop_length);
        add_content_of_type(content, ContentType.Model, path_crop_length);
        add_content_of_type(content, ContentType.Font, path_crop_length);
    }

    public static string ListAllContent() {
        string output = "";
        foreach (IContentData content in all_content.Values.Where(a => a.Loaded).OrderBy(a => a.FullName)) {
            output += $"{content.FullName} {(content.DataType == DataType.Procedural ? "[PROCEDURAL]" : "[ON DISK]")}\n";
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