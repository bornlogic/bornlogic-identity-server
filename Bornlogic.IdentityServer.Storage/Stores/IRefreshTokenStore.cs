﻿using Bornlogic.IdentityServer.Storage.Models;

namespace Bornlogic.IdentityServer.Storage.Stores
{
    /// <summary>
    /// Interface for refresh token storage
    /// </summary>
    public interface IRefreshTokenStore
    {
        /// <summary>
        /// Stores the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken);

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken);

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle);

        /// <summary>
        /// Removes the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        Task RemoveRefreshTokenAsync(string refreshTokenHandle);

        /// <summary>
        /// Removes the refresh tokens.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        Task RemoveRefreshTokensAsync(string subjectId, string clientId);
    }
}