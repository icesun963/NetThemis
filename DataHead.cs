namespace NetThemis
{
    /// <summary>
    /// �Զ���ڵ㶨��
    /// </summary>
    public enum DataHead
    {

        /// <summary>
        /// ������
        /// </summary>
        Null = 0,

        /// <summary>
        /// ������ǻ���������Ϊ�ֵ�����
        /// </summary>
        Dic = 1,

        /// <summary>
        /// ������������
        /// Array,����1,����1....����100,����100,ArrayEnd
        /// </summary>
        Array = 2,

        /// <summary>
        /// ͬ��������
        /// ArraryT,����,����,����1,����2
        /// </summary>
        ArrayT = 3,

        /// <summary>
        /// ���������
        /// </summary>
        ArrayEnd = 4,

        /// <summary>
        /// ����Json��ʽ�Ķ�����
        /// </summary>
        Bson = 5,
        /// <summary>
        /// �㼶��ʼ(��һ�㲻Ƕ��,�ڶ���Ƕ�����Ϳ�ʼ)
        /// </summary>
        LvStart=6,
        /// <summary>
        /// �㼶����
        /// </summary>
        LvEnd=7,

        //���´���������
        ArrayII = 12,
        ArrayIIT = 13,
        LvStartII = 16,
    }
}