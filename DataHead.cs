namespace NetThemis
{
    /// <summary>
    /// 自定义节点定义
    /// </summary>
    public enum DataHead
    {

        /// <summary>
        /// 空数据
        /// </summary>
        Null = 0,

        /// <summary>
        /// 如果不是基本数据则为字典数据
        /// </summary>
        Dic = 1,

        /// <summary>
        /// 随意类型数据
        /// Array,类型1,数据1....类型100,数据100,ArrayEnd
        /// </summary>
        Array = 2,

        /// <summary>
        /// 同类型数组
        /// ArraryT,类型,长度,数据1,数据2
        /// </summary>
        ArrayT = 3,

        /// <summary>
        /// 数组结束符
        /// </summary>
        ArrayEnd = 4,

        /// <summary>
        /// 类似Json格式的二进制
        /// </summary>
        Bson = 5,
        /// <summary>
        /// 层级开始(第一层不嵌套,第二层嵌套类型开始)
        /// </summary>
        LvStart=6,
        /// <summary>
        /// 层级结束
        /// </summary>
        LvEnd=7,

        //以下带长度描述
        ArrayII = 12,
        ArrayIIT = 13,
        LvStartII = 16,
    }
}