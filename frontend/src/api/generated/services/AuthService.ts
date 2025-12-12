/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { LoginRequestDto } from '../models/LoginRequestDto';
import type { LoginResultDtoApiResponse } from '../models/LoginResultDtoApiResponse';
import type { RefreshTokenRequestDto } from '../models/RefreshTokenRequestDto';
import type { RegisterUserDto } from '../models/RegisterUserDto';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AuthService {
    /**
     * @param requestBody
     * @returns LoginResultDtoApiResponse OK
     * @throws ApiError
     */
    public static postApiAuthLogin(
        requestBody?: LoginRequestDto,
    ): CancelablePromise<LoginResultDtoApiResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/login',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param requestBody
     * @returns LoginResultDtoApiResponse OK
     * @throws ApiError
     */
    public static postApiAuthRegister(
        requestBody?: RegisterUserDto,
    ): CancelablePromise<LoginResultDtoApiResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/register',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param requestBody
     * @returns LoginResultDtoApiResponse OK
     * @throws ApiError
     */
    public static postApiAuthRefresh(
        requestBody?: RefreshTokenRequestDto,
    ): CancelablePromise<LoginResultDtoApiResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Auth/refresh',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
