using AutoMapper;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateBookDto, Book>()
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => true));

        CreateMap<UpdateBookDto, Book>();

        CreateMap<Book, BookListDto>()
            .ForMember(dest => dest.AuthorName,
                opt => opt.MapFrom(src => src.Author.FirstName + " " + src.Author.LastName))
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name));
    }
}