using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository,
            IMapper mapper,
            IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUser = await _userRepository.GetUserByUserNameAsync(User.GetUsername());
            userParams.CurrentUsername = currentUser.UserName;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }
            PagedList<MemberDto> allUsers
                = await _userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(
                new PaginationHeader(
                    allUsers.CurrentPage,
                    allUsers.PageSize,
                    allUsers.TotalCount,
                    allUsers.TotalPages));
            return Ok(allUsers);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            AppUser user = await _userRepository.GetUserByIdAsync(id);
            MemberDto mappedUser = _mapper.Map<MemberDto>(user);
            return Ok(mappedUser);
        }

        [HttpGet("{username:alpha}")]
        public async Task<ActionResult<MemberDto>> GetUserByUserName(string username)
        {
            return await _userRepository
                .GetMemberByMemberNameAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            AppUser user = await _userRepository
                .GetUserByUserNameAsync(User.GetUsername() ?? "");
            if (user == null) return NotFound();
            _mapper.Map(memberUpdateDto, user);
            if (await _userRepository.SaveAllAsync())
                return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            AppUser user = await _userRepository
                .GetUserByUserNameAsync(User.GetUsername());
            if (User == null) return NotFound();
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            Photo photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0) photo.IsMain = true;
            user.Photos.Add(photo);
            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUserByUserName),
                    new { username = user.UserName },
                    _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            AppUser user = await _userRepository
                .GetUserByUserNameAsync(User.GetUsername());
            if (user is null) return NotFound();
            var photo = user.Photos
                .FirstOrDefault(photo => photo.Id == photoId);
            if (photo is null) return NotFound();
            if (photo.IsMain) return BadRequest("There is already a main photo");
            var currentMainPhoto = user.Photos
                .FirstOrDefault(photo => photo.IsMain);
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;
            photo.IsMain = true;
            if (await _userRepository.SaveAllAsync())
                return NoContent();
            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            AppUser user = await _userRepository
                .GetUserByUserNameAsync(User.GetUsername());
            if (user is null) return NotFound();
            Photo photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
            if (photo is null) return NotFound();
            if (photo.IsMain)
                return BadRequest("You can not delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null)
                    return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await _userRepository.SaveAllAsync())
                return Ok();
            return BadRequest("Problem deleting photo");
        }
    }
}