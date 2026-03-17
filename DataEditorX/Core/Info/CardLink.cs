/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2017/5/11
 * 时间: 16:14
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace DataEditorX.Core.Info
{
    /// <summary>
    /// Link Monster arrow directions (8 possible directions)
    /// </summary>
    [Flags]
    public enum CardLink : long
    {
        None = 0x0,
        
        ///<summary>↙</summary>
        DownLeft = 0x1,
        
        ///<summary>↓</summary>
        Down = 0x2,
        
        ///<summary>↘</summary>
        DownRight = 0x4,
        
        ///<summary>←</summary>
        Left = 0x8,
        
        ///<summary>→</summary>
        Right = 0x20,
        
        ///<summary>↖</summary>
        UpLeft = 0x40,
        
        ///<summary>↑</summary>
        Up = 0x80,
        
        ///<summary>↗</summary>
        UpRight = 0x100
    }
}
