using System;
using System.Collections.Generic;

namespace Masuit.Tools.Models
{
    /// <summary>
    /// 分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedList<T>
    {
        /// <summary>
        /// 数据集
        /// </summary>
        public List<T> Data { get; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// 当前页数据条数
        /// </summary>
        public int CurrentCount => Data.Count;

        /// <summary>
        /// 是否有前一页
        /// </summary>
        public bool HasPrev => CurrentPage > 1;

        /// <summary>
        /// 是否有后一页
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages;

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="items">数据集</param>
        /// <param name="page">当前页</param>
        /// <param name="size">页大小</param>
        /// <param name="count">总条数</param>
        public PagedList(List<T> items, int page, int size, int count)
        {
            TotalCount = count;
            PageSize = size;
            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(count * 1.0 / size);
            Data = items;
        }
    }
}
