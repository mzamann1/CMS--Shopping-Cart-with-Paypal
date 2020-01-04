﻿using MIS_CMS.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MIS_CMS.Models.ViewModels.Shop
{
    public class ProductVM
    {
        public ProductVM()
        {
                
        }

        public ProductVM(ProductDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Slug = dto.Slug;
            Description = dto.Description;
            Price = dto.Price;
            CategoryName = dto.CategoryName;
            CategoryId = dto.CategoryId;
            ImageName = dto.ImageName;
        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required]
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public string ImageName { get; set; }


        public IEnumerable<SelectListItem> Categories { get; set; }

        public IEnumerable<string> GalleryImages { get; set; }
                                           // public virtual CategoryDTO Category { get; set; }
    }
}