namespace OkeyApi.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OkeyApi.Dtos.Compte;
using OkeyApi.Interfaces;
using OkeyApi.Models;

[Route("okeyapi/compte")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<Utilisateur> _utilisateurManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<Utilisateur> _signInManager;

    public AccountController(
        UserManager<Utilisateur> utilisateurManager,
        ITokenService tokenService,
        SignInManager<Utilisateur> signInManager
    )
    {
        this._utilisateurManager = utilisateurManager;
        this._tokenService = tokenService;
        this._signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var user = await this._utilisateurManager.Users.FirstOrDefaultAsync(x =>
            x.UserName == loginDto.UserName.ToLower()
        );
        if (user == null)
            return this.Unauthorized("Nom d'utilisateur invalide");
        var result = await this._signInManager.CheckPasswordSignInAsync(
            user,
            loginDto.Password,
            false
        );
        if (!result.Succeeded)
            return this.Unauthorized(
                "Utilisateur non trouvé! (nom d'utilisateur ou mot de passe incorrect)"
            );

        return this.Ok(
            new NewUtilisateurDto
            {
                UserName = user.UserName,
                Token = this._tokenService.CreateToken(user)
            }
        );
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var utilisateur = new Utilisateur { UserName = registerDto.Username };
            var createUtilisateur = await this._utilisateurManager.CreateAsync(
                utilisateur,
                registerDto.Password
            );
            if (createUtilisateur.Succeeded)
            {
                var roleResult = await this._utilisateurManager.AddToRoleAsync(utilisateur, "USER");
                if (roleResult.Succeeded)
                {
                    return this.Ok(
                        new NewUtilisateurDto
                        {
                            UserName = utilisateur.UserName,
                            Token = this._tokenService.CreateToken(utilisateur)
                        }
                    );
                }
                return this.StatusCode(500, roleResult.Errors);
            }
            return this.StatusCode(500, createUtilisateur.Errors);
        }
        catch (Exception e)
        {
            return this.StatusCode(500, e);
        }
    }
}