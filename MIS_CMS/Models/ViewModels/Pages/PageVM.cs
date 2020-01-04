using MIS_CMS.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MIS_CMS.Models.ViewModels.Pages
{
    public class PageVM
    {
        public PageVM()
        {

        }

        public PageVM(PageDTO dto)
        {
            Id = dto.Id;
            Title = dto.Title;
            Slug = dto.Slug;
            Body = dto.Body;
            Sorting = dto.Sorting;
            HasSidebar = dto.HasSidebar;
        }
        public int Id { get; set; }
        [Required]
        [StringLength(50,MinimumLength =3)]
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue,MinimumLength =3)]
        public string Body { get; set; }
        public int Sorting { get; set; }
        public bool HasSidebar { get; set; }
    }
}