import {SearchableQuery} from "./searchable.query";

export interface LoadReportDto {
  
    id : string;
    number : number;
    name : string;
    status : string;
    createdAt : string;
    deliveredAt : string | null;
    deliveryCost : number;
    distance : number;
    truckNumber : string | null;
    customerName : string | null;
} 

export interface LoadsReportDto {
  items : LoadReportDto[];
  totalCount : number;
  TotalRevenue : number;
  totalDistance : number;
}

export interface DriversReportDto {
  items : DriverReportDto[];
  totalCount : number;
  totalGross : number;
  totalDistance : number;

}
export interface DriverReportDto {
  driverId : number;
  driverName : string;
  loadsDelivered : number;
  distanceDriven : number;
  grossEarnings : number;
}

export interface FinancialReportDto {
  invoiceId : string;
  invoiceNumber : number;
  status : string;
  total : number;
  paid : number;
  due : number;
  dueDate : string | null;
  customerName : string | null;
}
export interface FinancialsReportDto {
  
  items : FinancialReportDto[];
  totalCount : number;
  totalPaid : number;
  totalDue : number;
  totalInvoiced : number;
}

export interface GetLoadsReportQuery extends SearchableQuery {
  dateFrom?: string;
  dateTo?: string;
  loadStatus?: string;
  customerId?: string;
  dispatcherId?: string;
  format? : string
}
