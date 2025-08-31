import {HttpResponse, HttpHeaders} from "@angular/common/http";
import {Observable} from "rxjs";
import {map} from "rxjs/operators";
import {ApiBase} from "../api-base";
import {
  Result,
  PagedResult,
  SearchableQuery,
  GetLoadsReportQuery,
  LoadsReportDto,
  DriversReportDto,
  FinancialsReportDto,
} from "../models";

export interface GetLoadsReportExportQuery extends GetLoadsReportQuery {
  format: "csv" | "xlsx" | "pdf";
}

export class ReportApiService extends ApiBase {
  exportLoadsReport(query?: GetLoadsReportQuery): Observable<HttpResponse<Blob>> {
    return this.get(`/reports/loads?${this.stringfyQuery(query)}`, 
      {observe: "response", responseType: "blob"});
  }

  exportDriversReport(format: string): Observable<HttpResponse<Blob>> {
    return this.get(`/reports/drivers/export?format=${format}`, {
      observe: "response",
      responseType: "blob"
    });
  }

  getLoadsReport(query?: SearchableQuery): Observable<Result<LoadsReportDto>> {
    return this.get(`/reports/loads?${this.stringfySearchableQuery(query)}`);
  }
  getDriversReport(query?: SearchableQuery): Observable<Result<DriversReportDto>> {
    return this.get(`/reports/drivers?${this.stringfySearchableQuery(query)}`);
  }

  getFinancialsReport(query?: SearchableQuery): Observable<Result<FinancialsReportDto>> {
    return this.get(`/reports/financials?${this.stringfySearchableQuery(query)}`);
  }
}
