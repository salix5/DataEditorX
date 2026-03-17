using System;

namespace DataEditorX.Core.Info
{
    [Flags]
    public enum CardRule : long
    {
        /// <summary>无</summary>
        None = 0,
        /// <summary>OCG</summary>
        OCG = 0x1,
        /// <summary>TCG</summary>
        TCG = 0x2,
        /// <summary>DIY,原创卡</summary>
        DIY = 0x4,
        /// <summary>简体中文</summary>
        CCG = 0x8,
    }
}
