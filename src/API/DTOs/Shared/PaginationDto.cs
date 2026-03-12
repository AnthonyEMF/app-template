namespace API.DTOs.Shared
{
    public class PaginationDto<T>
    {
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
		public int TotalPages { get; set; }
		public int TotalItems { get; set; }
		public T Items { get; set; }
	}
}
