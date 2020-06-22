using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SYE.Helpers;
using SYE.Models;

namespace SYE.ViewModels
{
    public class SearchResultsVM
    {
        public SearchResultsVM()
        {
            Search = string.Empty;
        }

        [BindProperty(SupportsGet = true)]

        public bool ShowResults { get; set; }
        public int CurrentPage { get; set; } = 1;

        public long Count { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Count, PageSize));
        public bool ShowPrev => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;
        public bool ShowIncompletedSearchMessage { get; set; }
        public bool ShowExceededMaxLengthMessage { get; set; }
        public string ErrorMessage { get; set; }

        [Required(ErrorMessage = "Please enter a search")]
        public string Search { get; set; }
        public List<SearchResult> Data { get; set; }
        public List<SelectItem> Facets { get; set; }
        public List<SelectItem> FacetsModal { get; set; }
        public string SelectedFacets
        {
            get
            {
                if (this.Facets == null)
                {
                    return null;
                }
                return string.Join(',', this.Facets.Where(x => x.Selected).Select(x => x.Text).ToList());
            }
        }
        public IEnumerable<string> TypeOfService { get; set; }
    }
}
