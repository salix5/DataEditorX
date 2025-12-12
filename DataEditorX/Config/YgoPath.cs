using System.IO;

namespace DataEditorX.Config
{
    public class YgoPath
    {
        public YgoPath(string _gamepath)
        {
            SetPath(_gamepath);
        }
        public void SetPath(string _gamepath)
        {
            gamepath = _gamepath;
            picpath = MyPath.Combine(gamepath, "pics");
            fieldpath = MyPath.Combine(picpath, "field");
            luapath = MyPath.Combine(gamepath, "script");
            ydkpath = MyPath.Combine(gamepath, "deck");
            replaypath = MyPath.Combine(gamepath, "replay");
        }
        /// <summary>游戏目录</summary>
        public string gamepath;
        /// <summary>大图目录</summary>
        public string picpath;
        /// <summary>场地图目录</summary>
        public string fieldpath;
        /// <summary>脚本目录</summary>
        public string luapath;
        /// <summary>卡组目录</summary>
        public string ydkpath;
        /// <summary>录像目录</summary>
        public string replaypath;

        public string GetImage<T>(T id) => MyPath.Combine(picpath, $"{id}.jpg");
        public string GetImageField<T>(T id) => MyPath.Combine(fieldpath, $"{id}.png");
        public string GetScript<T>(T id) => MyPath.Combine(luapath, $"c{id}.lua");
        public string GetYdk(string name)
        {
            return MyPath.Combine(ydkpath, $"{name}.ydk");
        }
        public string GetModuleScript(string modulescript)
        {
            return MyPath.Combine(luapath, $"{modulescript}.lua");
        }

        public string[] GetCardfiles<T>(T id) => new[] { GetImage(id), GetImageField(id), GetScript(id) };
    }
}
