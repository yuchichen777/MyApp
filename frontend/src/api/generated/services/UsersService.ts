/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ObjectApiResponse } from '../models/ObjectApiResponse';
import type { UserCreateDto } from '../models/UserCreateDto';
import type { UserDto } from '../models/UserDto';
import type { UserDtoPagedResult } from '../models/UserDtoPagedResult';
import type { UserUpdateDto } from '../models/UserUpdateDto';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class UsersService {
    /**
     * 取得使用者分頁列表
     * @param page
     * @param pageSize
     * @param keyword
     * @param sortBy
     * @param desc
     * @param role
     * @returns UserDtoPagedResult OK
     * @throws ApiError
     */
    public static getUsersPaged(
        page?: number,
        pageSize?: number,
        keyword?: string,
        sortBy?: string,
        desc?: boolean,
        role?: string,
    ): CancelablePromise<UserDtoPagedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Users/paged',
            query: {
                'Page': page,
                'PageSize': pageSize,
                'Keyword': keyword,
                'SortBy': sortBy,
                'Desc': desc,
                'role': role,
            },
        });
    }
    /**
     * @param id
     * @returns UserDto OK
     * @throws ApiError
     */
    public static getApiUser(
        id: number,
    ): CancelablePromise<UserDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Users/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @param requestBody
     * @returns UserDto OK
     * @throws ApiError
     */
    public static putApiUsers(
        id: number,
        requestBody?: UserUpdateDto,
    ): CancelablePromise<UserDto> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Users/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @returns ObjectApiResponse OK
     * @throws ApiError
     */
    public static deleteApiUsers(
        id: number,
    ): CancelablePromise<ObjectApiResponse> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Users/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param requestBody
     * @returns UserDto OK
     * @throws ApiError
     */
    public static postApiUsers(
        requestBody?: UserCreateDto,
    ): CancelablePromise<UserDto> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Users',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiUsersMe(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Users/me',
        });
    }
}
