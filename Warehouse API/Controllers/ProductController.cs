﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Warehouse_API.DTO;
using Warehouse_API.Helpers;
using Warehouse_API.Interfaces;
using Warehouse_API.Models;

namespace Warehouse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult GetAllProducts() 
        {
            ICollection<ProductDTO> products = _mapper.Map<ICollection<ProductDTO>>(_productRepository.GetAllProducts());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(products);
        }
        
        [HttpGet("{productId}")]
        [ProducesResponseType(200, Type = typeof(Product))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public IActionResult GetProduct(int productId) 
        {
            if (!_productRepository.ProductExists(productId)) 
            {
                ModelState.AddModelError("Id", "A product with that id doesnt exist");
                return NotFound(ModelState);
            }

            ProductDTO products = _mapper.Map<ProductDTO>(_productRepository.GetProduct(productId));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public IActionResult CreateProduct([FromBody] ProductDTO productCreate)
        {
            if (productCreate == null)
            {
                return BadRequest(ModelState);
            }
            
            if (productCreate.ProductId != 0)
            {
                ModelState.AddModelError("Id", "The Id field should not be provided.");
                return BadRequest(ModelState);
            }
            bool SKUExists = _productRepository.GetAllProducts()
                                            .Where(p => p.SKU == productCreate.SKU)
                                            .Any();
            if (SKUExists)
            {
                ModelState.AddModelError("SKU", "Product with that SKU already exists");
                return StatusCode(409, ModelState);
            }

            if (!SKUHelper.IsValidSKU(productCreate.SKU) && productCreate.SKU != 0) 
            {
                ModelState.AddModelError("SKU", "SKU was not 8 digits");
                return BadRequest(ModelState);
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productMap = _mapper.Map<Product>(productCreate);

            if (!_productRepository.CreateProduct(productMap))
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok("Succesfully created product");
        }

        [HttpDelete("{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public IActionResult DeleteProduct(int productId)
        {
            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("Id", "A product with that id doesnt exist");
                return NotFound();
            }

            // Here you would also check if any other data is tied to this category and handle it

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_productRepository.DeleteProduct(productId))
            {
                ModelState.AddModelError("", "Something went wrong deleting product");
            }

            return Ok("Succesfully deleted product");

        }

        [HttpPut("{productId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateProduct(int productId, [FromBody] ProductDTO updatedProduct)
        {
            if (updatedProduct == null)
            {
                return BadRequest(ModelState);
            }

            if (productId != updatedProduct.ProductId)
            {
                ModelState.AddModelError("Id", "The query id doesnt match body id.");
                return BadRequest(ModelState);
            }

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("Id", "A product with that id doenst exist");
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Product productMap = _mapper.Map<Product>(updatedProduct);

            if (!_productRepository.UpdateProduct(productMap))
            {
                ModelState.AddModelError("", "Something went wrong updating product");
                return StatusCode(500, ModelState);
            }

            return Ok("Succesfully updated product");
        }
    }
}