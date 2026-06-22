import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AssetResponseDTO {
  id: number;
  assetname: string;
  assettypeId?: number;
  purchasedate?: string;
  warrantyexpiry?: string;
  status?: string;
  assignedClassId?: number;
}

export interface AssetTypeResponseDTO {
  id: number;
  typename: string;
}

export interface CreateAssetDTO {
  assetname: string;
  assettypeId?: number;
  purchasedate?: string;
  warrantyexpiry?: string;
  status?: string;
  assignedClassId?: number;
}

export interface AssetIssueDTO {
  assetId: number;
  status: string;
  report?: string;
}

export interface AssetReportResponseDTO {
  id: number;
  assetid: number;
  status?: string;
  report?: string;
  createdat?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AssetService {
  private readonly baseUrl = 'http://localhost:5203/api/Assets';

  constructor(private http: HttpClient) {}

  getAssets(): Observable<AssetResponseDTO[]> {
    return this.http.get<AssetResponseDTO[]>(this.baseUrl);
  }

  getAssetTypes(): Observable<AssetTypeResponseDTO[]> {
    return this.http.get<AssetTypeResponseDTO[]>(`${this.baseUrl}/types`);
  }

  addAsset(dto: CreateAssetDTO): Observable<AssetResponseDTO> {
    return this.http.post<AssetResponseDTO>(this.baseUrl, dto);
  }

  reportAssetIssue(dto: AssetIssueDTO): Observable<AssetReportResponseDTO> {
    return this.http.post<AssetReportResponseDTO>(`${this.baseUrl}/report`, dto);
  }

  getAssetReports(): Observable<AssetReportResponseDTO[]> {
    return this.http.get<AssetReportResponseDTO[]>(`${this.baseUrl}/reports`);
  }
}
