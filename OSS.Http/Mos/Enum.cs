namespace OSS.Http.Mos
{
    /// <summary>
    /// 返回的状态
    /// </summary>
    public enum ResponseStatus
    {
        /// <summary>
        /// 没有响应
        /// </summary>
        None,
        /// <summary>
        /// 响应ok
        /// </summary>
        Completed,
        /// <summary>
        /// 响应出错
        /// </summary>
        Error,
        /// <summary>
        /// 响应出错但正确返回数据
        /// </summary>
        ErrorButResponse,
        /// <summary>
        /// 超时
        /// </summary>
        TimedOut,
        /// <summary>
        /// 取消
        /// </summary>
        Aborted
    }
}
