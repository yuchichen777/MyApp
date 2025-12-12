/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CustomerCreateDto } from '../models/CustomerCreateDto';
import type { CustomerDto } from '../models/CustomerDto';
import type { CustomerDtoPagedResult } from '../models/CustomerDtoPagedResult';
import type { CustomerUpdateDto } from '../models/CustomerUpdateDto';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class CustomersService {
    /**
     * @returns CustomerDto OK
     * @throws ApiError
     */
    public static getApiCustomers(): CancelablePromise<Array<CustomerDto>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Customers',
        });
    }
    /**
     * @param requestBody
     * @returns CustomerDto OK
     * @throws ApiError
     */
    public static postApiCustomers(
        requestBody?: CustomerCreateDto,
    ): CancelablePromise<CustomerDto> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Customers',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @returns CustomerDto OK
     * @throws ApiError
     */
    public static getApiCustomer(
        id: number,
    ): CancelablePromise<CustomerDto> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Customers/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @param requestBody
     * @returns CustomerDto OK
     * @throws ApiError
     */
    public static putApiCustomers(
        id: number,
        requestBody?: CustomerUpdateDto,
    ): CancelablePromise<CustomerDto> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Customers/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiCustomers(
        id: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Customers/{id}',
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
     * @returns CustomerDtoPagedResult OK
     * @throws ApiError
     */
    public static getApiCustomersPaged(
        page?: number,
        pageSize?: number,
        keyword?: string,
        sortBy?: string,
        desc?: boolean,
    ): CancelablePromise<CustomerDtoPagedResult> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Customers/paged',
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
