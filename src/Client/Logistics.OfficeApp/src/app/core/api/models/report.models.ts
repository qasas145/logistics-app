import {SearchableQuery} from "./searchable.query";

export interface LoadsReportDto {
  id: string;
  name: string;
  type: string;
  status: string;
  customer: {
    id: string;
    name: string;
  };
  assignedDispatcher: {
    id: string;
    fullName: string;
  };
  originAddress: string;
  destinationAddress: string;
  deliveryCost: number;
  distance: number;
  createdAt: string;
  completedAt?: string;
}

export interface DriversReportDto {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  totalTrips: number;
  completedTrips: number;
  totalDistance: number;
  totalEarnings: number;
}

export interface FinancialsReportDto {
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;
  topCustomers: {
    customerId: string;
    customerName: string;
    totalSpent: number;
    totalLoads: number;
  }[];
  monthlyRevenue: {
    month: string;
    revenue: number;
  }[];
}

export interface GetLoadsReportQuery extends SearchableQuery {
  dateFrom?: string;
  dateTo?: string;
  loadStatus?: string;
  customerId?: string;
  dispatcherId?: string;
}
