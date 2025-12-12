namespace MyApp.Application.DTOs;

public class PagedQueryDto
{
    // 第幾頁（1 起算）
    public int Page { get; set; } = 1;

    // 每頁幾筆
    public int PageSize { get; set; } = 20;

    // 關鍵字 (Code / Name 模糊查詢)
    public string? Keyword { get; set; }

    // 排序欄位：Code / Name / Price ...
    public string? SortBy { get; set; }

    // 是否倒序
    public bool Desc { get; set; } = false;
}

/// <summary>
/// 泛型分頁結果
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
