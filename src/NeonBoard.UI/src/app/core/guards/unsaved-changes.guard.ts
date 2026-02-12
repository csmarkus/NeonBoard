import { Observable } from 'rxjs';

export interface HasUnsavedChanges {
  hasUnsavedChanges(): boolean;
  confirmDiscard(): Observable<boolean>;
}

export const unsavedChangesGuard = (component: HasUnsavedChanges) => {
  if (!component.hasUnsavedChanges()) {
    return true;
  }

  return component.confirmDiscard();
};
