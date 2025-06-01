﻿namespace DigitalDelivery.Application.Models
{
    public class PaginationResponse<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }

        public PaginationResponse()
        {
            Items = new List<T>();
        }
    }
}
