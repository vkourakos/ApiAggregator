using ApiAggregator.Domain;
using ApiAggregator.Infrastructure.Responses;
using AutoMapper;

namespace ApiAggregator.Infrastructure.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<WeatherApiResponse, AggregatedData>()
            .ForMember(dest => dest.SourceApi, opt =>
                opt.MapFrom(src => "WeatherAPI.com"))
            .ForMember(dest => dest.Title, opt =>
                opt.MapFrom(src => $"Weather in {src.Location.Name}, {src.Location.Country}"))
            .ForMember(dest => dest.Content, opt =>
                opt.MapFrom(src =>
                    $"Currently {src.Current.TempC}°C ({src.Current.Condition.Text}). " +
                    $"Feels like {src.Current.FeelslikeC}°C. " +
                    $"Wind is {src.Current.WindKph} kph from the {src.Current.WindDir}."))
            .ForMember(dest => dest.Url, opt =>
                opt.MapFrom(src => "https://www.weatherapi.com/"))
            .ForMember(dest => dest.PublishedDate, opt =>
                opt.MapFrom(src => DateTimeOffset.FromUnixTimeSeconds(src.Location.LocaltimeEpoch).UtcDateTime));

        CreateMap<GitHubRepo, AggregatedData>()
            .ForMember(dest => dest.SourceApi, opt => opt.MapFrom(src => "GitHub"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Content, opt =>
                opt.MapFrom(src => src.Description ?? "No description available."))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.HtmlUrl))
            .ForMember(dest => dest.PublishedDate, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<Article, AggregatedData>()
            .ForMember(dest => dest.SourceApi, opt => opt.MapFrom(src => "NewsAPI"))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Content, opt =>
                opt.MapFrom(src => src.Description ?? "No description available."))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.PublishedDate, opt =>
                opt.MapFrom(src => src.PublishedAt));
    }
}
