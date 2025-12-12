/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ObjectApiResponse } from '../models/ObjectApiResponse';
import type { ProductCreateDto } from '../models/ProductCreateDto';
import type { ProductDto } from '../models/ProductDto';
import type { ProductDtoPagedResult } from '../models/ProductDtoPagedResult';
import type { ProductUpdateDto } from '../models/ProductUpdateDto';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ProductsService {
    /**
     * @returns ProductDto OK
     * @throws ApiError
     */
    public static getApiProducts(): CancelablePromise<Array<ProductDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Products',
        });
    }
    /**
     * @param requestBody
     * @returns ProductDto OK
     * @throws ApiError
     */
    public static postApiProducts(
        requestBody?: ProductCreateDto,
    ): CancelablePromise<ProductDto> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Products',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @returns ProductDto OK
     * @throws ApiError
     */
    public static getApiProduct(
        id: number,
    ): CancelablePromise<ProductDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Products/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @param requestBody
     * @returns ProductDto OK
     * @throws ApiError
     */
    public static putApiProducts(
        id: number,
        requestBody?: ProductUpdateDto,
    ): CancelablePromise<ProductDto> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Products/{id}',
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
    public static deleteApiProducts(
        id: number,
    ): CancelablePromise<ObjectApiResponse> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Products/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param page
     * @param pageSize
     * @param keyword
     * @param sortBy
     * @param desc
     * @returns ProductDtoPagedResult OK
     * @throws ApiError
     */
    public static getApiProductsPaged(
        page?: number,
        pageSize?: number,
        keyword?: string,
        sortBy?: string,
        desc?: boolean,
    ): CancelablePromise<ProductDtoPagedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Products/paged',
            query: {
                'Page': page,
                'PageSize': pageSize,
                'Keyword': keyword,
                'SortBy': sortBy,
                'Desc': desc,
            },
        });
    }
}
