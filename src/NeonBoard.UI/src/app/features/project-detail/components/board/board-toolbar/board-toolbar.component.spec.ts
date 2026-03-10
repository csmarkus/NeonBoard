import { initTestEnvironment } from '../../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { BoardToolbarComponent } from './board-toolbar.component';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { ProjectContext } from '../../../services/project-context.service';
import { Label } from '../../../models/label.model';

initTestEnvironment();

function createMockFacade(labels: Label[] = [], selectedIds: Set<string> = new Set()) {
  const labelsSignal = signal<Label[]>(labels);
  const selectedLabelIdsSignal = signal<Set<string>>(selectedIds);
  const isFilterActiveSignal = signal(selectedIds.size > 0);
  return {
    labels: labelsSignal.asReadonly(),
    selectedLabelIds: selectedLabelIdsSignal.asReadonly(),
    isFilterActive: isFilterActiveSignal.asReadonly(),
    toggleLabelFilter: vi.fn(),
    clearLabelFilter: vi.fn(),
    _labels: labelsSignal,
    _selectedLabelIds: selectedLabelIdsSignal,
    _isFilterActive: isFilterActiveSignal,
  };
}

describe('BoardToolbarComponent', () => {
  let fixture: ComponentFixture<BoardToolbarComponent>;
  let mockFacade: ReturnType<typeof createMockFacade>;

  beforeEach(() => {
    mockFacade = createMockFacade();

    TestBed.configureTestingModule({
      imports: [BoardToolbarComponent],
      providers: [
        provideRouter([]),
        { provide: BoardStateFacade, useValue: mockFacade },
        { provide: ProjectContext, useValue: {
          canEdit: () => true,
          isOwner: () => true,
          currentUserRole: () => 'Owner',
          projectId: () => 'p-1',
          shortId: () => 'abc1234',
          projectName: () => 'Test Project',
          project: () => null,
          boards: () => [],
          boardsLoaded: () => true,
        } },
      ],
    });

    fixture = TestBed.createComponent(BoardToolbarComponent);
    fixture.componentRef.setInput('shortId', 'p-short-1');
    fixture.componentRef.setInput('slug', 'sprint-board');
    fixture.detectChanges();
  });

  it('renders the settings link with correct aria-label', () => {
    const settingsLink = fixture.nativeElement.querySelector('[aria-label="Board settings"]');
    expect(settingsLink).toBeTruthy();
  });

  describe('when board has no labels', () => {
    it('renders a disabled "All cards" filter button', () => {
      const button = fixture.nativeElement.querySelector('button[disabled]');
      expect(button?.textContent?.trim()).toBe('All cards');
    });

    it('does not render the dropdown toggle button', () => {
      const dropdownTrigger = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      expect(dropdownTrigger).toBeFalsy();
    });
  });

  describe('when board has labels', () => {
    beforeEach(() => {
      mockFacade._labels.set([{ id: 'label-1', name: 'Bug', color: 'red' }]);
      fixture.detectChanges();
    });

    it('renders "All cards" on the filter button when no filter is active', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      expect(button?.textContent?.trim()).toBe('All cards');
    });

    it('opens the dropdown when filter button is clicked', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      button.click();
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('[aria-expanded="true"]')).toBeTruthy();
    });

    it('shows label names in the dropdown', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      button.click();
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('Bug');
    });

    it('calls toggleLabelFilter with the label id when a label chip is clicked', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      button.click();
      fixture.detectChanges();
      const labelButton = fixture.nativeElement.querySelector('[aria-pressed]');
      labelButton.click();
      expect(mockFacade.toggleLabelFilter).toHaveBeenCalledWith('label-1');
    });

    it('closes the dropdown when onDocumentClick is called', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      button.click();
      fixture.detectChanges();
      fixture.componentInstance.onDocumentClick();
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('[aria-expanded="true"]')).toBeFalsy();
    });
  });

  describe('when filter is active', () => {
    beforeEach(() => {
      mockFacade._labels.set([
        { id: 'label-1', name: 'Bug', color: 'red' },
        { id: 'label-2', name: 'Feature', color: 'blue' },
      ]);
      mockFacade._selectedLabelIds.set(new Set(['label-1']));
      mockFacade._isFilterActive.set(true);
      fixture.detectChanges();
    });

    it('shows "1 label" on the filter button', () => {
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      expect(button?.textContent?.trim()).toBe('1 label');
    });

    it('shows "2 labels" when two labels are selected', () => {
      mockFacade._selectedLabelIds.set(new Set(['label-1', 'label-2']));
      fixture.detectChanges();
      const button = fixture.nativeElement.querySelector('[aria-haspopup="true"]');
      expect(button?.textContent?.trim()).toBe('2 labels');
    });

    it('shows a clear filter button', () => {
      const clearButton = fixture.nativeElement.querySelector('[aria-label="Clear label filter"]');
      expect(clearButton).toBeTruthy();
    });

    it('calls clearLabelFilter when the clear button is clicked', () => {
      const clearButton = fixture.nativeElement.querySelector('[aria-label="Clear label filter"]');
      clearButton.click();
      expect(mockFacade.clearLabelFilter).toHaveBeenCalled();
    });
  });
});
