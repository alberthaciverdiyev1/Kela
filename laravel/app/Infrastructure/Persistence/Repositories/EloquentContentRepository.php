<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;

class EloquentContentRepository implements ContentRepository
{
    public function create(array $attributes): Content
    {
        return Content::create($attributes);
    }

    public function update(Content $content, array $attributes): Content
    {
        $content->update($attributes);
        return $content;
    }

    public function delete(Content $content): bool
    {
        return (bool) $content->delete();
    }

    public function find(int $id): ?Content
    {
        return Content::find($id);
    }
}
