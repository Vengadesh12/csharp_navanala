using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class MenuMappings
    {
        public static MenuItemDto ToDto(this Menu menu)
        {
            return new MenuItemDto
            {
                Id = menu.Id,
                MenuKey = menu.MenuKey,
                Label = menu.Label,
                Name = menu.Label,
                MenuName = menu.Label,
                Icon = menu.Icon ?? string.Empty,
                Route = menu.Route ?? string.Empty,
                GroupName = menu.GroupName ?? string.Empty,
                Description = menu.Description ?? string.Empty,
                OrderIndex = menu.OrderIndex,
                PermissionKey = menu.PermissionKey,
                DeletedFlag = menu.DeletedFlag
            };
        }

        public static List<MenuItemDto> ToDtoList(this IEnumerable<Menu> menus)
        {
            return menus.Select(m => m.ToDto()).ToList();
        }
    }
}
