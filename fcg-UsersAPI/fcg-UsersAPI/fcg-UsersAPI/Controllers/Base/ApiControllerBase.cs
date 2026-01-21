using Application.Dto.Response;
using Application.Dto.Result;
using Microsoft.AspNetCore.Mvc;

namespace fcg_UsersAPI.Controllers.Base;

/// <summary>
/// Classe base para controllers da API que fornece m�todos padronizados para retornar resultados
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Retorna uma resposta de sucesso com dados no formato ServiceResult
    /// </summary>
    /// <typeparam name="T">Tipo do dado retornado</typeparam>
    /// <param name="data">Dados a serem retornados</param>
    /// <returns>ActionResult com status 200 OK</returns>
    protected IActionResult SuccessResult<T>(T data)
    {
        if (data == null)
        {
            return Ok(new
            {
                data = default(T),
                succeeded = true,
                errors = new List<string>()
            });
        }

        return Ok(new
        {
            data,
            succeeded = true,
            errors = new List<string>()
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status BadRequest (400)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 400 BadRequest</returns>
    protected IActionResult ErrorResult<T>(params string[] errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "Ocorreu um erro desconhecido" };

        return BadRequest(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status BadRequest (400)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 400 BadRequest</returns>
    protected IActionResult ErrorResult<T>(IEnumerable<string> errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "Ocorreu um erro desconhecido" };

        return BadRequest(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status Unauthorized (401)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 401 Unauthorized</returns>
    protected IActionResult UnauthorizedResult<T>(params string[] errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "N�o autorizado" };

        return Unauthorized(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status Unauthorized (401)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 401 Unauthorized</returns>
    protected IActionResult UnauthorizedResult<T>(IEnumerable<string> errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "N�o autorizado" };

        return Unauthorized(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status NotFound (404)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 404 NotFound</returns>
    protected IActionResult NotFoundResult<T>(params string[] errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "Recurso n�o encontrado" };

        return NotFound(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Retorna uma resposta de erro com status NotFound (404)
    /// </summary>
    /// <typeparam name="T">Tipo do dado (ser� null)</typeparam>
    /// <param name="errors">Lista de erros</param>
    /// <returns>ActionResult com status 404 NotFound</returns>
    protected IActionResult NotFoundResult<T>(IEnumerable<string> errors)
    {
        var errorList = errors?.ToList() ?? new List<string> { "Recurso n�o encontrado" };

        return NotFound(new
        {
            data = default(T),
            succeeded = false,
            errors = errorList
        });
    }

    /// <summary>
    /// Processa um OperationResult e retorna a resposta apropriada
    /// </summary>
    /// <typeparam name="T">Tipo do dado no OperationResult</typeparam>
    /// <param name="result">Resultado da opera��o</param>
    /// <returns>ActionResult com o status apropriado</returns>
    protected IActionResult ProcessResult<T>(OperationResult<T> result)
    {
        if (result == null)
        {
            return ErrorResult<T>("Resultado da opera��o n�o pode ser nulo");
        }

        if (result.Succeeded)
        {
            return SuccessResult(result.Data);
        }

        return ErrorResult<T>(result.Errors);
    }

    /// <summary>
    /// Processa um OperationResult sem dados e retorna a resposta apropriada
    /// </summary>
    /// <param name="result">Resultado da opera��o</param>
    /// <returns>ActionResult com o status apropriado</returns>
    protected IActionResult ProcessResult(OperationResult result)
    {
        if (result == null)
        {
            return BadRequest(new
            {
                succeeded = false,
                errors = new[] { "Resultado da opera��o n�o pode ser nulo" }
            });
        }

        if (result.Succeeded)
        {
            return NoContent();
        }

        return BadRequest(new
        {
            succeeded = false,
            errors = result.Errors
        });
    }
}
