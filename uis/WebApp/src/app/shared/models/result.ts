export class Result<T> {
  public readonly isSuccess: boolean;
  public readonly error: string | null;
  public readonly data: T | null;

  private constructor(isSuccess: boolean, error: string | null, data: T | null) {
    this.isSuccess = isSuccess;
    this.error = error;
    this.data = data;
  }

  public static success<T>(value: T): Result<T> {
    return new Result(true, null, value);
  }

  public static failure<T>(error: string): Result<T> {
    return new Result<T>(false, error, null as T | null);
  }
}
