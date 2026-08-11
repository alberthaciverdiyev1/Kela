<?php

namespace App\Application\Content;

use App\Domain\Content\ContentRepository;

/**
 * Məzmun (Dərs/Quiz/PDF/Video/Link) use-case-ləri.
 * Web/API bu servisi çağırır — Content modelinə birbaşa toxunmaz.
 */
class ContentService
{
    public function __construct(private readonly ContentRepository $contents)
    {
    }

    /** Workspace-a əlavə edilə bilən məzmunlar: [contentId => title]. */
    public function allContentOptions(int $teacherId): array
    {
        return $this->contents->allForTeacher($teacherId);
    }
}
